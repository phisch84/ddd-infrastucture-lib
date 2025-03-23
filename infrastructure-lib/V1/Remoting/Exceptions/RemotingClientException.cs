using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    using V1.Exceptions;

    /// <summary>
    /// Indicates an exception while processing a remoted invocation on the client side
    /// </summary>
    public class RemotingClientException : InfrastructureException
    {
        public RemotingClientException(Exception inner) : this(Resources.Messages.RemotingClientException, inner)
        {
        }

        public RemotingClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
