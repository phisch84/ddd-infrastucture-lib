using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    using V1.Aspects;
    using V1.Exceptions;
    using V1.Resources;
    using V1.Shared.Services;

    /// <summary>
    /// Thrown if no instance of a class implementing a derivate of <see cref="IRemotingClient"/> was registered as singleton via <see cref="ObjectFactory.RegisterSingleton{T}(T)"/>,
    /// but it is used as parameter of <see cref="RemotedAspect.RemotedAspect(Type)"/>.
    /// </summary>
    public class NoClientRegisteredException : InfrastructureException
    {
        /// <summary>
        /// Creates an instance of the exception
        /// </summary>
        /// <param name="interfaceName">
        /// The name of the interface that has been requested in <see cref="RemotedAspect.RemotedAspect(Type)"/>,
        /// but for which no instance was registered via <see cref="ObjectFactory.RegisterSingleton{T}(T)"/>.
        /// </param>
        public NoClientRegisteredException(string interfaceName) : base(String.Format(Messages.NoClientRegisteredException, interfaceName))
        {
        }
    }
}
