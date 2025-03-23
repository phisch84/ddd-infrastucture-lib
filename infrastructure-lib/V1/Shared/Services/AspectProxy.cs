using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Services
{
    using com.schoste.ddd.Infrastructure.V1.Aspects;
    using com.schoste.ddd.Infrastructure.V1.Shared.Exceptions;

    /// <summary>
    /// Version 1 of the proxy/wrapper than enables Aspect Oriented Programming (AOP) in this application.
    /// It will search the implementing classes and their members for attributes (<see cref="Attribute"/>)
    /// and process them as aspects (<see cref="Aspects.IMethodAspect"/>).
    /// </summary>
    /// <typeparam name="T">The interface of the class for which the proxy is created</typeparam>
    public class AspectProxy<T> : DispatchProxy where T : class
    {
        static private object mapAspectsToMethodLock = new object();

        private T? decorated;
        private Type? interfaceType;
        private readonly IDictionary<MethodInfo, MethodInfo?> ifMethodsToClsMethods = new Dictionary<MethodInfo, MethodInfo?>();
        private readonly IDictionary<MethodInfo, IList<IMethodAspect>> methodAspects = new Dictionary<MethodInfo, IList<IMethodAspect>>();

        /// <summary>
        /// Creates a new instance for <typeparamref name="T"/> wrapped in this proxy.
        /// Any method calls to this instance will be intercepted by <see cref="AspectProxy{T}.Invoke(IMessage)"/>.
        /// </summary>
        /// <param name="decorated">The instance to create the proxy for</param>
        /// <param name="interfaceType">
        /// The interface for which <paramref name="decorated"/> is created.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="decorated"/> is null</exception>
        static public T? Create(T decorated, Type interfaceType)
        {
            if (decorated == null) throw new ArgumentNullException(nameof(decorated));

            var proxy = DispatchProxy.Create<T, AspectProxy<T>>() as AspectProxy<T>;

            if (!ReferenceEquals(proxy, null))
            {
                proxy.interfaceType = interfaceType;
                proxy.decorated = decorated;
            }

            return proxy as T;
        }

        /// <summary>
        /// Called when any method of the proxy's underlying class is invoked, allowing to
        /// manipulate the call itself and the returned value(s).
        /// </summary>
        /// <param name="methodInfo">Information about the invoking method</param>
        /// <returns>The results of the call as object, or null if there is no result</returns>
        /// <exception cref="V1.Exceptions.InfrastructureException">Thrown if any exception occurs</exception>
        protected override object? Invoke(MethodInfo? methodInfo, object?[]? args)
        {
            var stopWatch = new Stopwatch();
            var result = null as object;
            var exception = null as Exception;

            if (ReferenceEquals(methodInfo, null))
            {
                return null;
            }

            try
            {
                lock (mapAspectsToMethodLock) this.MapAspectsToMethod(methodInfo);

                foreach (var methodAspect in this.methodAspects[methodInfo])
                {
                    try
                    {
                        methodAspect.BeforeMethodCall(this.decorated, args, this.interfaceType, this.ifMethodsToClsMethods[methodInfo]);
                    }
                    catch (Exception ex)
                    {
                        throw new V1.Exceptions.InfrastructureException(ex);
                    }
                }

                var methodCallTime = DateTime.Now;

                try
                {
                    stopWatch.Start();

                    result = methodInfo.Invoke(this.decorated, args);
                }
                catch (Exception ex)
                {
                    exception = ex.InnerException;
                }
                finally
                {
                    stopWatch.Stop();
                }

                foreach (var methodAspect in this.methodAspects[methodInfo])
                {
                    try
                    {
                        methodAspect.AfterMethodCall(methodCallTime, stopWatch.Elapsed, this.decorated, args, ref result, ref exception, this.interfaceType, this.ifMethodsToClsMethods[methodInfo]);
                    }
                    catch (Exception ex)
                    {
                        throw new V1.Exceptions.InfrastructureException(ex);
                    }
                }
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new V1.Exceptions.InfrastructureException(ex);
            }

            if (exception != null) throw exception;
            else return result;
        }

        static private bool matchParamInfos(ParameterInfo[] ifParamInfos, ParameterInfo[] clsParamInfos)
        {
            if (ifParamInfos.Length != clsParamInfos.Length) return false;

            for (var i = 0; i < ifParamInfos.Length; i++)
            {
                var ifParamInfo = ifParamInfos[i];
                var clsParamInfo = clsParamInfos[i];

                if (!ifParamInfo.ParameterType.Equals(clsParamInfo.ParameterType)) return false;
            }

            return true;
        }

        virtual protected MethodInfo? GetClassMethodForInterfaceMethod(MethodInfo interfaceMethod)
        {
            if (this.decorated == null) throw new InvalidOperationException();

            try
            {
                var classMethods = this.decorated.GetType().GetMethods();
                var sameNameCM = classMethods.Where(m => m.Name.Equals(interfaceMethod.Name, StringComparison.Ordinal));
                var sameRetParam = sameNameCM.Where(m => matchParamInfos(new[] { interfaceMethod.ReturnParameter }, new[] { m.ReturnParameter }));
                var sameCallParam = sameRetParam.Where(m => matchParamInfos(interfaceMethod.GetParameters(), m.GetParameters())).ToList();

                if (sameCallParam.Count < 1) return null;
                if (sameCallParam.Count > 1) throw new AmbiguousMethodException(interfaceMethod, sameCallParam);

                var classMethod = sameCallParam.First();

                return classMethod;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new V1.Exceptions.InfrastructureException(ex);
            }
        }

        virtual protected void MapAspectsToMethod(MethodInfo interfaceMethod)
        {
            try
            {
                if (this.methodAspects.ContainsKey(interfaceMethod)) return;

                this.methodAspects[interfaceMethod] = new List<IMethodAspect>();

                var allCustomAttributes = new List<CustomAttributeData>(interfaceMethod.CustomAttributes);

                if (this.interfaceType != null)
                {
                    this.ifMethodsToClsMethods[interfaceMethod] = this.GetClassMethodForInterfaceMethod(interfaceMethod);

                    if (this.ifMethodsToClsMethods[interfaceMethod] != null) allCustomAttributes.AddRange(this.ifMethodsToClsMethods[interfaceMethod].CustomAttributes);
                }
                else
                {
                    this.ifMethodsToClsMethods[interfaceMethod] = null;
                }

                foreach (var customAttribute in allCustomAttributes)
                {
                    var aspect = customAttribute.Constructor.Invoke(customAttribute.ConstructorArguments.Select(arg => arg.Value).ToArray()) as IMethodAspect;

                    if (aspect != null)
                    {
                        aspect.ValidateMethod(interfaceMethod);

                        this.methodAspects[interfaceMethod].Add(aspect);
                    }
                }
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new V1.Exceptions.InfrastructureException(ex);
            }
        }
    }
}
