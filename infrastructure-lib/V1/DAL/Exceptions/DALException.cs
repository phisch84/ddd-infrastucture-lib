using System;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Exceptions
{
    using V1.Exceptions;
    using V1.Resources;

    /// <summary>
    /// Base class for exceptions thrown from the DAL
    /// </summary>
    public class DALException : InfrastructureException
    {
        public DALException(Exception inner) : base(Messages.DALException, inner)
        { }

        public DALException(string message) : base(message)
        { }

        public DALException(string message, Exception inner) : base(message, inner)
        { }
    }
}
