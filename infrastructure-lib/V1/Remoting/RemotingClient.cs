using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    using Remoting.Exceptions;
    using Remoting.Models;
    using V1.Shared.Services;
    using V1.Exceptions;
    using V1.Logging;

    /// <summary>
    /// Abstract implementation of <see cref="IRemotingClient"/>. Provides the protocol for call of a method
    /// for a given target. Its concrete implementing type needs to provide the logic for sending the serialized
    /// invocation to the receiving <see cref="RemotingServer"/> by implementing <see cref="SendInvocationAsync(long, byte[])"/>.
    /// </summary>
    abstract public class RemotingClient : IRemotingClient
    {
        /// <summary>
        /// Default time in milliseconds to wait for a response of the server before a <see cref="TimeoutException"/> is thrown
        /// </summary>
        public const int DEFAULT_RESPONSE_TIMEOUT_MS = 10000;

        static protected ILog? Log = Logging.Log.Instance;

        protected ISerializer Serializer;
        protected ConcurrentDictionary<long, RemoteInvocation?> ResponsesCache = new ConcurrentDictionary<long, RemoteInvocation?>();
        protected AutoResetEvent ResponseArrivedEvent = new AutoResetEvent(false);
        protected TimeSpan ResponseWaitTimeout;
        protected Task? ResponseReader = null;
        protected object ResponseReaderLock = new object();
        protected long ResponseWaiters = 0;

        /// <summary>
        /// Creates a new instance with an interface to the serializer and a default response timeout of <see cref="DEFAULT_RESPONSE_TIMEOUT_MS"/> milliseconds.
        /// </summary>
        /// <param name="serializer">The interface to the serializer to use</param>
        /// <param name="responseTimeoutMs">The default time (in milliseconds) to wait for a response from the server</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serializer"/> is null</exception>
        public RemotingClient(ISerializer serializer, int responseTimeoutMs = DEFAULT_RESPONSE_TIMEOUT_MS)
        {
            if (ReferenceEquals(null, serializer)) throw new ArgumentNullException(nameof(serializer));

            this.Serializer = serializer;
            this.ResponseWaitTimeout = TimeSpan.FromMilliseconds(responseTimeoutMs);
        }

        /// <inheritdoc/>
        virtual public object? Configuration { get; set; }

        /// <inheritdoc/>
        virtual public async Task StopServer()
        {
            await this.SendInvocationAsync(0, []);
        }

        /// <inheritdoc/>
        virtual public void Invoke(object?[]? args, out object? returnValue, out Exception? ex, Type interfaceType, MethodInfo remoteMethod)
        {
            Log?.Debug(Resources.Messages.RemotingClientInvoke, remoteMethod.Name, Logging.Log.GetObjectTypeFullName(interfaceType));

            var payload = this.SerializeInvocation(args, interfaceType, remoteMethod);
            var msgId = DateTime.Now.Ticks;

            try
            {
                this.ResponsesCache[msgId] = null;

                Log?.Debug(Resources.Messages.RemotingClientInvokeSendingInvocation, remoteMethod.Name, Logging.Log.GetObjectTypeFullName(interfaceType), msgId, payload.Length);

                this.SendInvocationAsync(msgId, payload);

                lock (this.ResponseReaderLock)
                {
                    this.ResponseWaiters++;

                    if (this.ResponseReader == null) this.ResponseReader = Task.Run(() => this.ReadNextResponses());
                }

                var response = this.WaitForResponse(msgId);

                this.DeserializeResponse(response, out returnValue, out ex);
            }
            catch (Exception unhandledEx)
            {
                Log?.Error(unhandledEx);

                throw new InfrastructureException(unhandledEx);
            }
        }

        virtual protected void DeserializeResponse(RemoteInvocation ri, out object? returnValue, out Exception? ex)
        {
            ex = null;

            if (!ReferenceEquals(null, ri.ThrownException))
            {
                var deserializedException = this.Serializer.Deserialize(ri.ThrownException);

                if (deserializedException.Length > 0)
                {
                    if (deserializedException[0] is RemoteMethodException) ex = deserializedException[0] as RemoteMethodException;
                    if (deserializedException[0] is RemotingServerException) ex = deserializedException[0] as RemotingServerException;

                    if (ReferenceEquals(null, ex)) ex = deserializedException[0] as Exception;
                }
                else
                {
                    ex = null;
                }
            }

            var deserializedRetVal = !ReferenceEquals(null, ri.ReturnValue) ? this.Serializer.Deserialize(ri.ReturnValue) : null;

            returnValue = (ReferenceEquals(null, deserializedRetVal) || deserializedRetVal.Length == 0) ? null : deserializedRetVal[0];
        }

        virtual protected byte[] SerializeInvocation(object?[]? args, Type interfaceType, MethodInfo remoteMethod)
        {
            var argsDTO = (args != null) ? this.Serializer.Serialize(args) : this.Serializer.Serialize([]);
            var rpcModel = new RemoteInvocation()
            {
                TargetInterfaceTypeGUID = interfaceType.GUID,
                TargetMethodName = remoteMethod.Name,
                MethodArguments = argsDTO,
            };

            return this.Serializer.Serialize(rpcModel);
        }

        virtual protected void ReadNextResponses()
        {
            while (true)
            {
                try
                {
                    var responseData = this.ReadNextResponse(out var msgId);
                    var response = this.ProcessResponse(responseData);

                    this.ResponsesCache[msgId] = response;

                    lock (this.ResponseReaderLock)
                    {
                        if (this.ResponseWaiters < 1)
                        {
                            this.ResponseReader = null;

                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log?.Error(ex);
                }
                finally
                {
                    this.ResponseArrivedEvent.Set();
                }
            }
        }

        virtual protected RemoteInvocation WaitForResponse(long msgId)
        {
            try
            {
                var waitingStartTime = DateTime.Now;

                while (this.ResponsesCache.TryGetValue(msgId, out var response))
                {
                    if (!ReferenceEquals(response, null)) return response;

                    var waitingTime = DateTime.Now - waitingStartTime;

                    if (waitingTime > this.ResponseWaitTimeout) break;

                    this.ResponseArrivedEvent.WaitOne(this.ResponseWaitTimeout);
                }

                throw new TimeoutException();
            }
            finally
            {
                lock (this.ResponseReaderLock) this.ResponseWaiters--;
            }
        }

        virtual protected RemoteInvocation ProcessResponse(byte[] responseMsg)
        {
            var dto = this.Serializer.Deserialize<DataTransferObject>(responseMsg);
            var response = this.Serializer.Deserialize(dto);

            if (ReferenceEquals(null, response) || (response.Length != 1)) throw new System.IO.InvalidDataException();

            var castResponse = response[0] as RemoteInvocation;

            if (ReferenceEquals(null, castResponse)) throw new System.IO.InvalidDataException();

            return castResponse;
        }

        /// <summary>
        /// Asnychronously sends a method invocation to the server
        /// </summary>
        /// <param name="msgId">Id of the message that is being sent and for which a response is awaited</param>
        /// <param name="invocation">Serialized data of the method invocation</param>
        /// <returns>A task that performs the transmission</returns>
        abstract protected Task SendInvocationAsync(long msgId, byte[] invocation);

        /// <summary>
        /// Called by <see cref="ReadNextResponses"/>.
        /// Reads the next response message from the stream.
        /// </summary>
        /// <param name="msgId">The id of the message for which the response is for</param>
        /// <returns>Serialized data of the response</returns>
        abstract protected byte[] ReadNextResponse(out long msgId);
    }
}
