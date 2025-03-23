using System;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Exceptions
{
    using Aspects;
    using V1.Exceptions;

    /// <summary>
    /// Thrown if a method cannot be used with a certain <see cref="IMethodAspect"/>
    /// </summary>
    public class InvalidMethodForAspectException : InfrastructureException
    {
        /// <summary>
        /// Gets information about the incompatible method
        /// </summary>
        public MethodInfo IncompatibleMethod { get; protected set; }

        /// <summary>
        /// Gets the interface to the aspect for which <see cref="IncompatibleMethod"/> cannot be used
        /// </summary>
        public IMethodAspect Aspect { get; protected set; }

        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="method">Sets <see cref="IncompatibleMethod"/></param>
        /// <param name="aspect">Sets <see cref="Aspect"/></param>
        public InvalidMethodForAspectException(MethodInfo method, IMethodAspect aspect) : this(method, aspect, String.Format(Resources.Messages.InvalidMethodForAspectException, method.Name, aspect.GetType().FullName))
        { }

        /// <summary>
        /// Creates a new instance of the class.
        /// Use this constructor for exceptions that derive from this one and set a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="method">Sets <see cref="IncompatibleMethod"/></param>
        /// <param name="aspect">Sets <see cref="Aspect"/></param>
        /// <param name="message">Sets <see cref="Exception.Message"/></param>
        public InvalidMethodForAspectException(MethodInfo method, IMethodAspect aspect, string message) : base(message)
        {
            this.IncompatibleMethod = method;
            this.Aspect = aspect;
        }

    }
}
