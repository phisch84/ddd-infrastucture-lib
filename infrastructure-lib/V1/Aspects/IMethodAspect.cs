using System;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using Shared.Exceptions;

    /// <summary>
    /// Attributes that implement this interface will be recognized by the <see cref="V1.Shared.Services.AspectProxy{T}"/> class.
    /// For instances that were created via <see cref="V1.Shared.Services.ObjectFactory.CreateInstance{T}(object[])"/> the method
    /// <see cref="BeforeMethodCall(object?[]?)"/> will be called before any method of the class is invoked, and
    /// <see cref="AfterMethodCall(DateTime, TimeSpan, object?[]?, object?, Exception?, Type?, MethodInfo?)"/> will be called after any method returns.
    /// </summary>
    public interface IMethodAspect
    {
        /// <summary>
        /// Invoked by the <see cref="V1.Shared.Services.AspectProxy{T}"/> when wrapping an aspect around a method.
        /// Checks if the <paramref name="method"/> fulfills all criteria (e.g. return type, parameters, etc.) to be compatible with the given aspect.
        /// </summary>
        /// <param name="method">Information of the method for which the aspect is initialized</param>
        /// <exception cref="InvalidMethodForAspectException">Thrown if the methód is incompatible with the aspect</exception>
        void ValidateMethod(MethodInfo method);

        /// <summary>
        /// Invoked by the <see cref="V1.Shared.Services.AspectProxy{T}"/> before <paramref name="implementingMethod"/>
        /// is called on <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The instance where <paramref name="implementingMethod"/> is called on</param>
        /// <param name="args">Any parameters passed to the called method</param>
        /// <param name="interfaceType">The type of the interface encapsulating the method</param>
        /// <param name="implementingMethod">Information of the actual method that is implementing (e.g., behind an interface)</param>
        void BeforeMethodCall(object? target, object?[]? args, Type? interfaceType = null, MethodInfo? implementingMethod = null);

        /// <summary>
        /// Invoked by the <see cref="V1.Shared.Services.AspectProxy{T}"/> after <paramref name="implementingMethod"/>
        /// was called on <paramref name="target"/>.
        /// </summary>
        /// <param name="methodCallTime">The time when the method was called</param>
        /// <param name="methodRunTime">The time span it took to execute the method</param>
        /// <param name="target">The instance where <paramref name="implementingMethod"/> is called on</param>
        /// <param name="args">Any parameters passed to the called method</param>
        /// <param name="returnValue">The return value of the method (if any)</param>
        /// <param name="ex">The exception that was thrown by the method (if any)</param>
        /// <param name="interfaceType">The type of the interface encapsulating the method</param>
        /// <param name="implementingMethod">Information of the actual method that is implementing (e.g., behind an interface)</param>
        void AfterMethodCall(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, ref object? returnValue, ref Exception? ex, Type? interfaceType = null, MethodInfo? implementingMethod = null);
    }
}
