using System;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Exceptions
{
    using V1.Aspects;
    using V1.Shared.Exceptions;

    /// <summary>
    /// Thrown if a method has a return type or a parameter without the <see cref="SerializableAttribute"/>
    /// and therefore cannot be used with the <see cref="RemotedAspect"/>
    /// </summary>
    public class TypeNotSerializableException : InvalidMethodForAspectException
    {
        /// <summary>
        /// Creates a new instance of the exception
        /// </summary>
        /// <param name="unserializableType">The type which lacks the <see cref="SerializableAttribute"/> and causes this exception</param>
        /// <param name="method">The method in which <paramref name="unserializableType"/> is used</param>
        /// <param name="aspect">The aspect which was wrapped around <paramref name="method"/></param>
        public TypeNotSerializableException(Type unserializableType, MethodInfo method, IMethodAspect aspect) : base(method, aspect, String.Format(Resources.Messages.TypeNotSerializableException, unserializableType.Name, method.Name, aspect.GetType().Name))
        { }
    }
}
