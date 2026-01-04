using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using Exceptions;
    using Logging;
    using Remoting;
    using Remoting.Exceptions;
    using Shared.Services;

    /// <summary>
    /// Aspect to decorate methods of classes for remoting.
    /// When methods with this attribute are called, their local implementation is executed first, and then
    /// the <see cref="AspectProxy{T}"/> will invoke <see cref="AfterMethodCall(DateTime, TimeSpan, object?, object?[]?, ref object?, ref Exception?, Type?, MethodInfo?)"/>
    /// of this attribute which will send the invocation to the implementation of <see cref="IRemotingClient"/> for execution on the remote endpoint.
    /// The implementation of <see cref="IRemotingClient"/> must have been registered via <see cref="ObjectFactory.RegisterSingleton{T}(T)"/> or
    /// a <see cref="NoClientRegisteredException"/> will be thrown at the construction of this attribute's instance.
    /// </summary>
    public class RemotedAspect : Attribute, IMethodAspect
    {
        static protected ILog? Log = Logging.Log.Instance;

        private IRemotingClient? remotingClient;

        static private T? genericTaskInvocationMethod<T>(IRemotingClient? remotingClient, object?[]? args, Type interfaceType, MethodInfo implementingMethod)
        {
            var returnValue = null as object;
            var ex = null as Exception;

            remotingClient?.Invoke(args, out returnValue, out ex, interfaceType, implementingMethod);

            if (!ReferenceEquals(ex, null)) throw ex;

            return (T?)returnValue;
        }

        static private object? createAsyncInvocationTask(IRemotingClient? remotingClient, object?[]? args, Type interfaceType, MethodInfo implementingMethod)
        {
            if (ReferenceEquals(null, remotingClient));

            var taskResultType = implementingMethod.ReturnType.GenericTypeArguments;
            var taskType = (taskResultType.Length < 1) ? typeof(Task<object?>)
                                                       : typeof(Task<>).MakeGenericType(taskResultType);
            var actionType = (taskResultType.Length < 1) ? new[] { typeof(object) }
                                                         : taskResultType;

            var exParamRemotingClient = Expression.Constant(remotingClient, typeof(IRemotingClient));
            var exParamArgs = Expression.Constant(args, typeof(object?[]));
            var exParamInterfaceType = Expression.Constant(interfaceType, typeof(Type));
            var exParamImplementingMethod = Expression.Constant(implementingMethod, typeof(MethodInfo));
            var exCallTaskMethod = Expression.Call(typeof(RemotedAspect), nameof(genericTaskInvocationMethod), actionType, new Expression[] { exParamRemotingClient, exParamArgs, exParamInterfaceType, exParamImplementingMethod });

            var action = Expression.Lambda(exCallTaskMethod).Compile();
            var task = Activator.CreateInstance(taskType, action);

            if (!ReferenceEquals(null, task)) ((Task) task).Start();

            return task;
        }

        /// <summary>
        /// Constrcutor of the attribute with no explicit interface to the remoting client specified.
        /// The aspect will look for the instance registered at the <see cref="ObjectFactory"/> for <see cref="IRemotingClient"/>.
        /// </summary>
        public RemotedAspect() : this(typeof(IRemotingClient))
        { }

        /// <summary>
        /// Constructor of the attribute with an explicit interface to the remoting client specified,
        /// therefore allowing the usage of multiple different remoting clients at once in an application.
        /// The aspect will look for the instance registered at the <see cref="ObjectFactory"/> for <paramref name="remotingClientInterface"/>.
        /// </summary>
        /// <param name="remotingClientInterface">The remoting client's interface that must be derived from <see cref="IRemotingClient"/></param>
        /// <exception cref="NoClientRegisteredException">Thrown if no remoting client was registered by the application via <see cref="ObjectFactory.RegisterSingleton{T}(T)"/></exception>
        /// <exception cref="InfrastructureException">Re-throws every exception</exception>
        public RemotedAspect(Type remotingClientInterface)
        {
            try
            {
                if (ReferenceEquals(remotingClientInterface, null)) throw new ArgumentNullException(nameof(remotingClientInterface));
                if (ReferenceEquals(remotingClient, null)) remotingClient = ObjectFactory.GetInstance<IRemotingClient>(remotingClientInterface);
                if (ReferenceEquals(remotingClient, null)) throw new NoClientRegisteredException(Logging.Log.GetObjectTypeFullName(remotingClientInterface));

                Log?.Debug(Resources.Messages.RemotedAspectInitialized, Logging.Log.GetObjectTypeFullName(remotingClient), Logging.Log.GetObjectTypeFullName(remotingClientInterface));
            }
            catch (NoClientRegisteredException)
            {
                throw;
            }
            catch (InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex);
            }

        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Checks if the method for which this aspect has been applied is compatible.
        /// Methods are compatible if all their input-parameter types and their return type have custom attribute <see cref="SerializableAttribute"/>.
        /// </summary>
        /// <param name="method">Info of the method to validate</param>
        /// <exception cref="ArgumentException">Thrown if the return type of <paramref name="method"/> isn't assignable to <see cref="System.Threading.Tasks.Task"/></exception>
        /// <exception cref="TypeNotSerializableException">Thrown if either one of the method's input-parameter types or its return type does not have the custom attribute <see cref="SerializableAttribute"/></exception>
        public void ValidateMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var typesToCheck = parameterTypes.ToList();

            if (!returnType.IsAssignableTo(typeof(System.Threading.Tasks.Task)))
            {
                var msg = String.Format(Resources.Messages.ValidateMethodNotATaskArgumentException, method.Name, returnType.FullName, typeof(System.Threading.Tasks.Task).FullName);

                throw new ArgumentException(msg, nameof(method));
            }
            else
            {
                typesToCheck.AddRange(returnType.GetGenericArguments());
            }

            foreach (var typeToCheck in typesToCheck)
            {
                if (typeof(void).Equals(typeToCheck)) continue; // void isn't serializable, but acceptable as return type
                if (typeToCheck.IsAssignableTo(typeof(System.Collections.IEnumerable))) continue; // IEnumerable doesn't have the SerializableAttribute, but is acceptable as return type

                var serializableAttr = typeToCheck.GetCustomAttribute(typeof(SerializableAttribute));

                if (ReferenceEquals(null, serializableAttr)) throw new TypeNotSerializableException(typeToCheck, method, this);
            }
        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Called by the <see cref="AspectProxy{T}"/> after a method with the <see cref="RemotedAspect"/> attribute has been executed.
        /// This method forwards the method call and its local result to the registered <see cref="IRemotingClient"/> for transmission
        /// to the remoting server and will return the result of the call on the remoting server
        /// </summary>
        /// <param name="methodCallTime">The time when the method was called</param>
        /// <param name="methodRunTime">The time span it took to execute the method</param>
        /// <param name="target">The instance where <paramref name="implementingMethod"/> is called on</param>
        /// <param name="args">Any parameters passed to the called method</param>
        /// <param name="returnValue">The return value of the method (if any)</param>
        /// <param name="ex">The exception that was thrown by the method (if any)</param>
        /// <param name="interfaceType">The type of the interface encapsulating the method</param>
        /// <param name="implementingMethod">Information of the actual method that is implementing (e.g., behind an interface)</param>
        /// <exception cref="InfrastructureException">Re-throws every <see cref="InfrastructureException"/></exception>
        /// <exception cref="RemotingClientException">Re-throws every general exception</exception>
        public void AfterMethodCall(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, ref object? returnValue, ref Exception? ex, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        {
            try
            {
                if (ReferenceEquals(null, implementingMethod)) throw new ArgumentNullException(nameof(implementingMethod));
                if (ReferenceEquals(null, interfaceType)) throw new ArgumentNullException(nameof(interfaceType));

                Log?.Debug(Resources.Messages.RemotedAspectAfterMethodCall, implementingMethod.Name, Logging.Log.GetObjectTypeFullName(target));

                returnValue = createAsyncInvocationTask(remotingClient, args, interfaceType, implementingMethod);
            }
            catch (Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception unhandledEx)
            {
                throw new RemotingClientException(unhandledEx);
            }
        }

        /// <summary>
        /// <inheritdoc/><br/>
        /// Does nothing.
        /// </summary>
        public void BeforeMethodCall(object? target, object?[]? args, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        { }
    }
}
