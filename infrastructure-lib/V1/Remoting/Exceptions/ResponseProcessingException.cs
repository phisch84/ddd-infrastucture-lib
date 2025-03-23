using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    /// <summary>
    /// Indicates an exception in the <see cref="RemotingServer"/> after returning from <see cref="RemotingServer.Invoke(Type, object, string, object[], out Exception?)"/>,
    /// meaning that there is an error in processing the response of the invocation.
    /// </summary>
    public class ResponseProcessingException : RemotingServerException
    {
        public ResponseProcessingException(Exception inner) : base(Resources.Messages.ResponseProcessingException, inner)
        { }
    }
}
