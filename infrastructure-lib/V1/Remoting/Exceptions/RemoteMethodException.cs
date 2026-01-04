using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    /// <summary>
    /// This exception encapsulates exceptions thrown by remote methods invoked via remoting.
    /// </summary>
    /// <remarks>
    /// Since remote methods can throw exceptions defined in their own assemblies, which may not be available, or
    /// the thrown exception contains data that cannot be serialized,
    /// this exception serves as a wrapper to convey that an error occurred
    /// </remarks>
    [Serializable]
    public class RemoteMethodException : RemotingServerException
    {
        /// <summary>
        /// Creates a new instance of the RemoteMethodException class based on the specified exception, recursively
        /// wrapping inner exceptions as RemoteMethodException instances.
        /// </summary>
        /// <param name="originalException">
        /// The original exception to wrap. If the exception has inner exceptions, they are also wrapped as RemoteMethodException instances.
        /// </param>
        /// <returns>A RemoteMethodException that wraps the specified exception and its inner exceptions.</returns>
        static public RemoteMethodException CreateFrom(Exception originalException)
        {
            if (ReferenceEquals(originalException.InnerException, null)) return new RemoteMethodException(originalException.Message);
            
            var inner = CreateFrom(originalException.InnerException);

            return new RemoteMethodException(originalException, inner);
        }

        /// <summary>
        /// Constructor for serializers
        /// </summary>
        public RemoteMethodException() : base()
        {
            this.Message = String.Empty;
        }

        /// <summary>
        /// Creates a new instance with a given message
        /// </summary>
        /// <param name="message">Sets the <see cref="Exception.Message"/> property</param>
        public RemoteMethodException(string message) : base(message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Creates a new instance with a given message from an original exception
        /// </summary>
        /// <param name="originalException">
        /// Sets the <see cref="RemoteMethodException.Message"/> property from the <see cref="Exception.Message"/> property of the given exception
        /// </param>
        public RemoteMethodException(Exception originalException) : base(originalException.Message)
        {
            this.Message = originalException.Message;
        }

        /// <summary>
        /// Creates a new instance with a given message and a given inner exception from an original exception
        /// </summary>
        /// <param name="originalException">
        /// Sets the <see cref="RemoteMethodException.Message"/> property from the <see cref="Exception.Message"/> property of the given exception
        /// </param>
        /// <param name="wrappedInnerException">Sets the <see cref="Exception.InnerException"/> property</param>
        public RemoteMethodException(Exception originalException, RemoteMethodException wrappedInnerException) : base(originalException.Message)
        {
            this.Message = originalException.Message;
        }
    }
}
