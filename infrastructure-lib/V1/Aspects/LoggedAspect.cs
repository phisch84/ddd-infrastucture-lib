using System;
using System.Linq;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using V1.Exceptions;
    using V1.Logging;
    using V1.Resources;

    /// <summary>
    /// Aspect to decorate methods of classes for logging when they are invoked.
    /// Invocations to methods with this attribute will be intercepted by the <see cref="Shared.Services.AspectProxy{T}"/>
    /// and the call will be logged by <see cref="Logging.Log"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class LoggedAspect : Attribute, IMethodAspect
    {
        static private string formatCallingMethodTrace(Type? implementingType, MethodInfo? implementingMethod)
        {
            var className = (implementingType == null) ? LogFormats.ImplementingTypeNULL : implementingType.FullName;

            if (implementingMethod == null) return String.Format(LogFormats.MethodInfo, String.Empty, className, LogFormats.MethodInfoNULL, String.Empty);

            var methodName = implementingMethod.Name;
            var returnType = implementingMethod.ReturnType.FullName;
            var paramNames = implementingMethod.GetParameters().Select(p => p.ParameterType.FullName);

            return String.Format(LogFormats.MethodInfo, returnType, className, methodName, String.Join(", ", paramNames));
        }

        static private string formatCallingMethodDebug(Type? implementingType, MethodInfo? implementingMethod)
        {
            var className = (implementingType == null) ? LogFormats.ImplementingTypeNULL : implementingType.FullName;

            if (implementingMethod == null) return String.Format(LogFormats.MethodInfo, String.Empty, className, LogFormats.MethodInfoNULL, String.Empty);

            var methodName = implementingMethod.Name;
            var returnType = implementingMethod.ReturnType.FullName;
            var paramNames = implementingMethod.GetParameters().Select(p => p.ParameterType.FullName);

            return String.Format(LogFormats.MethodInfo, returnType, className, methodName, String.Join(", ", paramNames));
        }

        static private void logAfterMethodCallTrace(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, object? returnValue, Exception? ex, Type? implementingType = null, MethodInfo? implementingMethod = null)
        {
            var methodInfo = formatCallingMethodTrace(implementingType, implementingMethod);

            Log.Instance?.Trace(LogFormats.LoggedAspectInfo, methodInfo, methodRunTime.ToString());
        }

        static private void logAfterMethodCallDebug(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, object? returnValue, Exception? ex, Type? implementingType = null, MethodInfo? implementingMethod = null)
        {
            var methodInfo = formatCallingMethodDebug(implementingType, implementingMethod);

            Log.Instance?.Debug(LogFormats.LoggedAspectInfo, methodInfo, methodRunTime.ToString());
        }

        /// <summary>
        /// Default contructor. Does nothing.
        /// </summary>
        public LoggedAspect()
        { }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Implementation of <see cref="IMethodAspect.ValidateMethod(MethodInfo)"/>.
        /// Does nothing.
        /// </summary>
        public void ValidateMethod(MethodInfo method)
        {

        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Implementation of <see cref="IMethodAspect.BeforeMethodCall(object?, object?[]?, Type?, MethodInfo?)"/>.
        /// Does nothing.
        /// </summary>
        public void BeforeMethodCall(object? target, object?[]? args, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        {
        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Writes a log messages depending on the log level.
        /// <list type="bullet">
        /// <item>For <see cref="Log.LogLevels.Trace"/> <see cref="Log.Trace(string, object[])"/> is called.</item>
        /// <item>For <see cref="Log.LogLevels.Debug"/> <see cref="Log.Debug(string, object[])"/> is called.</item>
        /// </list>
        /// </summary>
        /// <exception cref="InfrastructureException">Re-throws every exception</exception>
        public void AfterMethodCall(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, ref object? returnValue, ref Exception? ex, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        {
            try
            {
                if (ReferenceEquals(null, Log.Instance)) return;

                if (Log.Instance.Level.HasFlag(Log.LogLevels.Trace))
                {
                    logAfterMethodCallTrace(methodCallTime, methodRunTime, target, args, returnValue, ex, interfaceType, implementingMethod);
                }
                else if (Log.Instance.Level.HasFlag(Log.LogLevels.Debug))
                {
                    logAfterMethodCallDebug(methodCallTime, methodRunTime, target, args, returnValue, ex, interfaceType, implementingMethod);
                }
            }
            catch (Exception exc)
            {
                throw new InfrastructureException(exc);
            }
        }
    }
}
