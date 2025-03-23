using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    /// <summary>
    /// Indicates an exception in <see cref="RemotingServer"/> before calling <see cref="RemotingServer.Invoke(Type, object, string, object[], out Exception?)"/>,
    /// meaning there was an error in preparing the invocation.
    /// </summary>
    public class MessageProcessingException : RemotingServerException
    {
        public MessageProcessingException(Exception inner) : base(inner)
        {
        }
    }
}
