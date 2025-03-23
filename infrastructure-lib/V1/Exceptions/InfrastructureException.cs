using System;

namespace com.schoste.ddd.Infrastructure.V1.Exceptions
{
    using V1.Resources;

    /// <summary>
    /// Base class for exceptions thrown from the Infrastructure layer
    /// </summary>
    public class InfrastructureException : Exception
    {
        public InfrastructureException(Exception inner) : base(Messages.InfrastructureException, inner)
        { }

        public InfrastructureException(string message) : base(message)
        { }

        public InfrastructureException(string message, Exception inner) : base(message, inner)
        { }
    }
}
