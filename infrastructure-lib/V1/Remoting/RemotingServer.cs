using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    using Logging;
    using Remoting.Models;
    using Shared.Exceptions;
    using Shared.Services;

    /// <summary>
    /// Astract implementation of the <see cref="IRemotingServer"/> interface. Knows the protocol for call of a method
    /// for a given target. Its concrete implementing type needs to provide the logic for sending back the serialized
    /// response to the receiving <see cref="RemotingClient"/> by implementing <see cref="SendResponse(long, DataTransferObject)"/>.
    /// </summary>
    abstract public class RemotingServer : IRemotingServer
    {
        static protected ILog? Log = Logging.Log.Instance;

        protected ISerializer Serializer;
        protected Task? MessageReaderTask = null;
        protected CancellationTokenSource? CancellationTokenSource = null;
        protected readonly IDictionary<Guid, Type> ClsGuidsToTypesMap = new Dictionary<Guid, Type>();
        protected readonly ConcurrentDictionary<long, Task> InvocationTasks = new ConcurrentDictionary<long, Task>();

        /// <summary>
        /// Creates a new instance with an interface to the serializer.
        /// </summary>
        /// <param name="serializer">The interface to the serializer to use</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serializer"/> is null</exception>
        public RemotingServer(ISerializer serializer)
        {
            if (ReferenceEquals(serializer, null)) throw new ArgumentNullException(nameof(serializer));

            this.Serializer = serializer;
        }

        /// <inheritdoc/>
        virtual public object? Configuration { get; set; }

        /// <inheritdoc/>
        virtual public Task Start()
        {
            this.CancellationTokenSource = new CancellationTokenSource();
            this.MessageReaderTask = Task.Factory.StartNew(() => this.Run(this.CancellationTokenSource.Token));

            return this.MessageReaderTask;
        }

        /// <inheritdoc/>
        virtual public void Stop()
        {
            this.CancellationTokenSource?.Cancel();
            this.MessageReaderTask = null;
        }

        virtual protected void Run(CancellationToken cancellationToken)
        {
            long msgId;
            byte[] msg;

            Log?.Debug(Resources.Messages.RemotingServerRunStarted, Logging.Log.GetObjectTypeFullName(this));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    msgId = this.ReadNextMessage(out msg);

                    Log?.Debug(Resources.Messages.RemotingServerRunMsgReceived, msgId, msg.Length);

                    if (msg.Length < 1) break;
                    if (this.InvocationTasks.ContainsKey(msgId)) continue;
                }
                catch (Exceptions.RemotingServerException ex)
                {
                    Log?.Error(ex);

                    throw;
                }
                catch (Exception ex)
                {
                    Log?.Error(ex);

                    throw new Exceptions.MessageProcessingException(ex);
                }

                try
                {
                    var invocationTask = Task.Factory.StartNew(() => this.ProcessInvocation(msgId, msg), cancellationToken);

                    this.InvocationTasks.TryAdd(msgId, invocationTask);
                }
                catch (Exceptions.RemotingServerException ex)
                {
                    Log?.Error(ex);

                    throw;
                }
                catch (Exception ex)
                {
                    Log?.Error(ex);

                    throw new Exceptions.ResponseProcessingException(ex);
                }
            }

            Log?.Debug(Resources.Messages.RemotingServerRunStopping, this.InvocationTasks.Count);

            Task.WaitAll(this.InvocationTasks.Values.ToArray());

            Log?.Debug(Resources.Messages.RemotingServerRunStopped);
        }

        virtual protected async Task ProcessInvocation(long msgId, byte[] msg)
        {
            try
            {
                var response = await ProcessMessage(msgId, msg);

                Log?.Debug(Resources.Messages.RemotingServerProcessInvocationSendingResponse, msgId);

                try
                {
                    await this.SendResponse(msgId, response);
                }
                catch (Exception ex)
                {
                    throw new Exceptions.ResponseProcessingException(ex);
                }
            }
            catch (Exceptions.RemotingServerException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exceptions.RemotingServerException(ex);
            }
            finally
            {
                this.InvocationTasks.TryRemove(msgId, out var invocationTask);
            }
        }

        virtual protected async Task<DataTransferObject> ProcessMessage(long msgId, byte[] msg)
        {
            RemoteInvocation? ri;
            Type type;
            object? target;
            object[]? args;

            try
            {
                Log?.Debug(Resources.Messages.RemotingServerProcessMessageStart, msgId);

                ri = this.Serializer.Deserialize<RemoteInvocation>(msg);

                Log?.Debug(Resources.Messages.RemotingServerProcessMessageDeserialized, msgId, (ReferenceEquals(null, ri.TargetMethodName) ? String.Empty : ri.TargetMethodName), ri.TargetInterfaceTypeGUID);

                if (String.IsNullOrEmpty(ri.TargetMethodName)) throw new InvalidDataException();
                if (ReferenceEquals(null, ri.MethodArguments)) throw new InvalidDataException();

                type = this.LookUpType(ri.TargetInterfaceTypeGUID);

                if (ReferenceEquals(null, type)) throw new InterfaceNotFoundException(ri.TargetInterfaceTypeGUID.ToString());

                target = this.LookUpSingleton(type);

                if (ReferenceEquals(null, target)) throw new ClassNotFoundException(Logging.Log.GetObjectTypeFullName(type));

                args = (ri.MethodArguments.Data == null || ri.MethodArguments.Data.Length < 1) ? new object[0]
                       : this.Serializer.Deserialize(ri.MethodArguments);
            }
            catch (Exception ex)
            {
                throw new Exceptions.MessageProcessingException(ex);
            }

            try
            {
                var thrownException = null as Exception;
                var retVal = await Task.Run(() => this.Invoke(type, target, ri.TargetMethodName, args, out var thrownException));

                Log?.Debug(Resources.Messages.RemotingServerProcessMessageInvoked, msgId, Logging.Log.GetObjectTypeFullName(retVal), Logging.Log.GetObjectTypeFullName(thrownException));

                var response = new RemoteInvocation()
                {
                    MethodArguments = this.Serializer.Serialize(args),
                    ThrownException = thrownException,
                    ReturnValue = !ReferenceEquals(null, retVal) ? this.Serializer.Serialize([retVal]) : this.Serializer.Serialize([]),
                };

                var dto = this.Serializer.Serialize([response]);

                return dto;
            }
            catch (Exception ex)
            {
                throw new Exceptions.ResponseProcessingException(ex);
            }
        }

        virtual protected object? Invoke(Type type, object target, string methodName, object[] args, out Exception? thrownException)
        {
            try
            {
                thrownException = null;

                Log?.Debug(Resources.Messages.RemotingServerInvoke, methodName, Logging.Log.GetObjectTypeFullName(target));

                return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, target, args, null);
            }
            catch (Exception ex)
            {
                thrownException = ex;

                return null;
            }
        }

        virtual protected Type LookUpType(Guid classId)
        {
            if (this.ClsGuidsToTypesMap.ContainsKey(classId)) return this.ClsGuidsToTypesMap[classId];

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var typesOfAssemblies = assemblies.SelectMany(asm => asm.GetTypes()).ToList();
            var type = typesOfAssemblies.FirstOrDefault(t => t.GUID == classId);

            if (type == null) throw new ClassNotFoundException(classId.ToString());

            this.ClsGuidsToTypesMap[classId] = type;

            return type;
        }

        virtual protected object? LookUpSingleton(Type interfaceType)
        {
            if (ObjectFactory.Configuration.InterfaceToInstanceMap.TryGetValue(interfaceType, out var instance)) return instance;
            else return null;
        }

        /// <summary>
        /// Reads the next message of the method invocation
        /// </summary>
        /// <param name="msg">The serialized data of the method and its parameters to invoke</param>
        /// <returns>The id of the message provided by the <see cref="RemotingClient"/></returns>
        abstract protected long ReadNextMessage(out byte[] msg);

        /// <summary>
        /// Asnychronously sends back the result of a method invocation to a <see cref="RemotingClient"/>.
        /// </summary>
        /// <param name="msgId">The id of the message from the <see cref="RemotingClient"/> the server is answering to</param>
        /// <param name="response">Serialized data of the result</param>
        /// <returns>The task which performs the transmission</returns>
        abstract protected Task SendResponse(long msgId, DataTransferObject response);

        #region Dispose()
        private bool disposedValue;

        virtual protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RemotingServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
