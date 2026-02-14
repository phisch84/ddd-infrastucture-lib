using System;
using System.Reflection;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    using Aspects;
    using Shared.Services;

    /// <summary>
    /// Interface to an implementation of <see cref="RemotingClient"/>. When such an implmementation is registered
    /// at <see cref="ObjectFactory.RegisterSingleton{T}(T)"/>, then its instance is used by <see cref="RemotedAspect"/>
    /// to forward method invocations to a corresponding <see cref="RemotingServer"/>.
    /// </summary>
    public interface IRemotingClient
    {
        /// <summary>
        /// Gets or sets the configuration of the remoting client.
        /// What the actual configuration looks like, depends on the client implementation
        /// </summary>
        object? Configuration { get; set; }

        /// <summary>
        /// Called by the <see cref="RemotedAspect"/> to transfer the call to the <see cref="RemotingServer"/> and wait for
        /// the return value of the call
        /// </summary>
        /// <param name="args">Arguments to be passed to the invoked method. If null, the method expects no arguments.</param>
        /// <param name="returnValue">The return value of the invoked method. If null, the method returned void</param>
        /// <param name="ex">An exception thrown by the invoked method. If null, no exception was thrown.</param>
        /// <param name="interfaceType">The <see cref="Type"/> of the interface of the class which implements the method</param>
        /// <param name="implementingMethod">Information about the method to invoke</param>
        /// <exception cref="V1.Exceptions.InfrastructureException">Re-throws all exceptions</exception>
        void Invoke(object?[]? args, out object? returnValue, out Exception? ex, Type interfaceType, MethodInfo implementingMethod);

        /// <summary>
        /// Wraps the <see cref="Invoke(object?[], out object?, out Exception?, Type, MethodInfo)"/> method and returns the return value of the call directly,
        /// as well as it will rethrow any exception as <see cref="Exceptions.RemoteMethodException"/> thrown by the invoked method, instead of returning it as an out parameter.
        /// This method is intended to be used by clients which want to call the <see cref="RemotingServer"/> directly, without using the <see cref="RemotedAspect"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the return value</typeparam>
        /// <param name="args">Arguments to be passed to the invoked method. If null, the method expects no arguments.</param>
        /// <param name="interfaceType">The <see cref="Type"/> of the interface of the class which implements the method</param>
        /// <param name="implementingMethod">Information about the method to invoke</param>
        /// <returns>The value returned by the remotely invoked method</returns>
        /// <exception cref="Exceptions.RemoteMethodException">Encapsulates any exception that was thrown by the remote method</exception>
        T? Invoke<T>(object?[]? args, Type interfaceType, MethodInfo implementingMethod) where T : class;

        /// <summary>
        /// Sends a stop message to the <see cref="IRemotingServer"/>.
        /// </summary>
        Task StopServer();
    }
}
