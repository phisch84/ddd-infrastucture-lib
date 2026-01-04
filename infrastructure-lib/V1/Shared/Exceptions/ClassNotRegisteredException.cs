using com.schoste.ddd.Infrastructure.V1.Exceptions;
using com.schoste.ddd.Infrastructure.V1.Resources;
using System;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Exceptions
{
    /// <summary>
    /// Indicates that a given class or <see cref="Type"/> the property <see cref="Type.GUID"/> is <see cref="Guid.Empty"/>.
    /// </summary>
    /// <remarks>
    /// This exception is typically thrown in the following situations.
    /// <list type="bullet">
    ///     <item>
    ///     When calling <see cref="Infrastructure.V1.Shared.Services.ISerializer.Serialize(object?[])"/> and one of the objects to be serialized doesn't have a valid GUID.
    ///     A possible remedy is to register a GUID for the class using <see cref="Infrastructure.V1.Shared.Services.ISerializer.RegisterGuidForType(Guid, Type)"/>.
    ///     </item>
    /// </list>
    /// </remarks>
    public class ClassNotRegisteredException : InfrastructureException
    {
        public ClassNotRegisteredException(string classFullName) : base(String.Format(Messages.ClassNotRegisteredException, classFullName))
        { }
    }
}
