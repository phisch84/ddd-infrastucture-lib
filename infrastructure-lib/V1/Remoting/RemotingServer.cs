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
    using Remoting.Exceptions;
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

        static private async Task<object?>? toTaskWithResult(Task task)
        {
            var voidTaskResultType = Type.GetType("System.Threading.Tasks.VoidTaskResult");
            if (ReferenceEquals(null, voidTaskResultType) || voidTaskResultType.IsAssignableFrom(task.GetType())) throw new InvalidOperationException();

            var voidTaskType = typeof(Task<>).MakeGenericType(voidTaskResultType);
            if (voidTaskType.IsAssignableFrom(task.GetType())) return null;

            var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return null;

            await task;

            return property.GetValue(task);
        }

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
                DataTransferObject? response = null;
                Exception? processMessageException = null;

                // Deserialize the invocation, invoke the requested method and serialize its response
                try
                {
                    response = await ProcessMessage(msgId, msg);
                }
                catch (ResponseProcessingException rpe)
                {
                    processMessageException = (ReferenceEquals(null, rpe.InnerException)) ? rpe : rpe.InnerException;
                }
                catch (Exception ex)
                {
                    processMessageException = ex;
                }

                // Send back the serialized response - if available, or send back the exception
                // that prevented the proper response to be formed, so clients don't keep waiting
                try
                {
                    if (ReferenceEquals(response, null))
                    {
                        var rse = new RemotingServerException(processMessageException.Message);
                        var serializedVoid = this.Serializer.Serialize([]);
                        var serializedException = ReferenceEquals(null, processMessageException)
                                                ? this.Serializer.Serialize([])
                                                : this.Serializer.Serialize([new RemotingServerException(processMessageException.Message)]);
                        var exceptionResponse = new RemoteInvocation()
                        {
                            MethodArguments = serializedVoid,
                            ThrownException = serializedException,
                            ReturnValue = serializedVoid,
                            
                        };

                        response = this.Serializer.Serialize([exceptionResponse]);
                    }

                    Log?.Debug(Resources.Messages.RemotingServerProcessInvocationSendingResponse, msgId);

                    await this.SendResponse(msgId, response);
                }
                catch (Exceptions.RemotingServerException rse)
                {
                    Log?.Error(rse);

                    throw;
                }
                catch (Exception ex)
                {
                    Log?.Error(ex);

                    throw new Exceptions.ResponseProcessingException(ex);
                }
            }
            finally
            {
                this.InvocationTasks.TryRemove(msgId, out var invocationTask);
            }
        }

        virtual protected void DeserializeMessage(long msgId, byte[] msg, out RemoteInvocation ri)
        {
            try
            {
                Log?.Debug(Resources.Messages.RemotingServerProcessMessageStart, msgId);

                ri = this.Serializer.Deserialize<RemoteInvocation>(msg);

                Log?.Debug(Resources.Messages.RemotingServerProcessMessageDeserialized, msgId, (ReferenceEquals(null, ri.TargetMethodName) ? String.Empty : ri.TargetMethodName), ri.TargetInterfaceTypeGUID);
            }
            catch (Exception ex)
            {
                Log?.Error(ex);

                throw new Exceptions.MessageProcessingException(ex);
            }
        }

        virtual protected void GetInvocationParameters(RemoteInvocation ri, out Type type, out object target, out object[]? args)
        { 
            try
            {
                type = this.LookUpType(ri.TargetInterfaceTypeGUID);

                if (ReferenceEquals(null, type)) throw new InterfaceNotFoundException(ri.TargetInterfaceTypeGUID.ToString());

                var targetForType = this.LookUpSingleton(type);

                if (ReferenceEquals(null, targetForType)) throw new ClassNotFoundException(Logging.Log.GetObjectTypeFullName(type));

                target = targetForType;
                args = (ri.MethodArguments == null || ri.MethodArguments.Data == null || ri.MethodArguments.Data.Length < 1)
                     ? new object[0] : this.Serializer.Deserialize(ri.MethodArguments);
            }
            catch (Exception ex)
            {
                Log?.Error(ex);

                throw new Exceptions.MessageProcessingException(ex);
            }
        }

        virtual protected Task<object?>? InvokeMethodAsync(string methodName, Type type, object target, object[]? args)
        {
            try
            {
                Log?.Debug(Resources.Messages.RemotingServerInvoke, methodName, Logging.Log.GetObjectTypeFullName(target));

                // Since methods with the RemotedAspect attribute must return Task or Task<T>, we can safely cast here
                var invocationTask = type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, target, args, null) as Task;

                if (ReferenceEquals(null, invocationTask)) throw new ArgumentNullException(nameof(invocationTask));

                var taskWithResult = toTaskWithResult(invocationTask);

                return taskWithResult;
            }
            catch (Exception ex)
            {
                Log?.Error(ex);

                throw new Exceptions.MessageProcessingException(ex);
            }
        }

        virtual protected DataTransferObject SerializeResponse(object? returnedValue, Exception? thrownException = null)
        {
            try
            {
                var serializedMethodArguments = this.Serializer.Serialize([]);
                var serializedReturnValue = !ReferenceEquals(null, returnedValue) ? this.Serializer.Serialize([returnedValue]) : this.Serializer.Serialize([]);
                var serializedThrownException = !ReferenceEquals(null, thrownException) ? this.Serializer.Serialize([thrownException]) : this.Serializer.Serialize([]);
                var response = new RemoteInvocation()
                {
                    MethodArguments = serializedMethodArguments,
                    ThrownException = serializedThrownException,
                    ReturnValue = serializedReturnValue,
                };

                var dto = this.Serializer.Serialize([response]);

                return dto;
            }
            catch (Exception ex)
            {
                Log?.Error(ex);

                throw new Exceptions.ResponseProcessingException(ex);
            }
        }

        virtual protected async Task<DataTransferObject> ProcessMessage(long msgId, byte[] msg)
        {
            this.DeserializeMessage(msgId, msg, out var ri);

            if (String.IsNullOrEmpty(ri.TargetMethodName)) throw new InvalidDataException();
            if (ReferenceEquals(null, ri.MethodArguments)) throw new InvalidDataException();

            this.GetInvocationParameters(ri, out var type, out var target, out var args);

            var taskWithResult = this.InvokeMethodAsync(ri.TargetMethodName, type, target, args);

            RemoteMethodException? thrownException = null;
            object? returnedValue = null;

            try
            {
                returnedValue = (ReferenceEquals(null, taskWithResult)) ? null : await taskWithResult;
            }
            catch (Exception ex)
            {
                // some fields of an exception may not be serializable, so create a shallow clone of it
                thrownException = RemoteMethodException.CreateFrom(ex);
            }
            finally
            {
                Log?.Debug(Resources.Messages.RemotingServerProcessMessageInvoked, msgId, Logging.Log.GetObjectTypeFullName(returnedValue), Logging.Log.GetObjectTypeFullName(thrownException));
            }

            var dto = this.SerializeResponse(returnedValue, thrownException);

            return dto;
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
