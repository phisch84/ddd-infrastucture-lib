using System;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Exceptions
{
    using V1.Exceptions;
    using V1.Resources;

    /// <summary>
    /// Indicates that a given interface that should be registered to the <see cref="Shared.Services.ObjectFactory"/> class
    /// does not exist.
    /// </summary>
    public class InterfaceNotFoundException : InfrastructureException
    {
        public InterfaceNotFoundException(string interfaceFullName) : base(String.Format(Messages.InterfaceNotFoundException, interfaceFullName))
        { }
    }
}
