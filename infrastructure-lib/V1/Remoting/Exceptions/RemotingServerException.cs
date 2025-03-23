using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    using Infrastructure.V1.Exceptions;

    /// <summary>
    /// Indicates an exception while processing a remoted invocation on the server side
    /// </summary>
    public class RemotingServerException : InfrastructureException
    {
        public RemotingServerException(Exception inner) : base(Resources.Messages.RemotingServerException, inner)
        { }

        public RemotingServerException(string message) : base(message)
        { }

        public RemotingServerException(string message, Exception inner) : base(message, inner)
        { }
    }
}
