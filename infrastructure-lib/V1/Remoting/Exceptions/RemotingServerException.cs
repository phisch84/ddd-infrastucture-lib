using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    using Infrastructure.V1.Exceptions;

    /// <summary>
    /// Indicates an exception while processing a remoted invocation on the server side
    /// </summary>
    public class RemotingServerException : InfrastructureException
    {
        static protected void SetMessage(Exception ex, string msg)
        {
            var msgFieldInfo = ex.GetType().GetField("_message", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                msgFieldInfo?.SetValue(ex, msg);
        }

        /// <summary>
        /// Gets or sets the error message that describes the current exception.
        /// </summary>
        /// <remarks>
        /// The original <see cref="Exception.Message"/> property is read-only, therefore the message is
        /// set via reflection (<see cref="SetMessage(Exception, string)"/>).
        /// </remarks>
        new public string Message
        {
            get { return base.Message; }
            set { SetMessage(this, value); }
        }

        /// <summary>
        /// Constructor for serializers
        /// </summary>
        public RemotingServerException() : base(Resources.Messages.RemotingServerException)
        { }

        public RemotingServerException(Exception inner) : base(Resources.Messages.RemotingServerException, inner)
        { }

        public RemotingServerException(string message) : base(message)
        { }

        public RemotingServerException(string message, Exception inner) : base(message, inner)
        { }
    }
}
