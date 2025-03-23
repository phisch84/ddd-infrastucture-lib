using System;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Exceptions
{
    using V1.Exceptions;
    using V1.Resources;

    /// <summary>
    /// Indicates that a given interface that should be registered to the <see cref="Shared.Services.ObjectFactory"/> class
    /// does not exist.
    /// </summary>
    public class ClassNotFoundException : InfrastructureException
    {
        public ClassNotFoundException(string classFullName) : base(String.Format(Messages.ClassNotFoundException, classFullName))
        { }
    }
}
