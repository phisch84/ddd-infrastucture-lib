using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Exceptions
{
    using V1.Exceptions;
    using V1.Resources;

    /// <summary>
    /// Thrown if for a method in an interface more than one matching methods were found.
    /// </summary>
    public class AmbiguousMethodException : InfrastructureException
    {
        public MethodInfo InterfaceMethod { get; protected set; }

        public IEnumerable<MethodInfo> ClassMethods { get; protected set; }

        public AmbiguousMethodException(MethodInfo interfaceMethod, IEnumerable<MethodInfo> classMethods) : base(String.Format(Messages.AmbiguousMethodException))
        {
            InterfaceMethod = interfaceMethod;
            ClassMethods = new List<MethodInfo>(classMethods);
        }
    }
}
