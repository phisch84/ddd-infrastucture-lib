using System;
using System.Reflection;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Services
{
    using V1.Shared.Models;

    /// <summary>
    /// Abstract factory to create instances of classes for instances, which are decorated by <see cref="AspectProxy{T}"/>.
    /// </summary>
    public class ObjectFactory
    {
        /// <summary>
        /// Gets the configuration model of the object factory
        /// </summary>
        static public ObjectFactoryConfiguration Configuration { get; } = new ObjectFactoryConfiguration();

        static private Type getTypeFromAssemblyForClass(string className)
        {
            var type = Assembly.GetExecutingAssembly().GetType(className);

            if (type != null) return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(className);

                if (type != null) return type;
            }

            throw new Exceptions.ClassNotFoundException(className);
        }

        /// <summary>
        /// Maps the full name of the implementing class (<paramref name="implementingClass"/>) to the full name of its
        /// interface (<paramref name="clsInterface"/>) so that it can be used later with <see cref="CreateInstance{T}(object[])"/>
        /// </summary>
        /// <param name="clsInterface">The interface to register</param>
        /// <param name="implementingClass">The class to map to the interface</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="clsInterface"/> or <paramref name="implementingClass"/> is null</exception>
        /// <exception cref="Exceptions.InterfaceNotFoundException">Thrown if the full name for <paramref name="clsInterface"/> cannot be found</exception>
        /// <exception cref="Exceptions.ClassNotFoundException">Thrown if the full name for <paramref name="implementingClass"/> cannot be found</exception>
        static public void Register(Type clsInterface, Type implementingClass)
        {
            if (ReferenceEquals(clsInterface, null)) throw new ArgumentNullException(nameof(clsInterface));
            if (ReferenceEquals(implementingClass, null)) throw new ArgumentNullException(nameof(implementingClass));

            var ifFullName = clsInterface.FullName;
            var clsFullName = implementingClass.FullName;

            if (String.IsNullOrEmpty(ifFullName)) throw new Exceptions.InterfaceNotFoundException(clsInterface.Name);
            if (String.IsNullOrEmpty(clsFullName)) throw new Exceptions.ClassNotFoundException(clsInterface.Name);

            Configuration.InterfaceToImplementationMap[ifFullName] = clsFullName;
        }

        /// <summary>
        /// Checks if an implementing class is registered for a given interface
        /// </summary>
        /// <param name="clsInterface">The interface to check if an implementation was registered for</param>
        /// <returns>True if there is a class registered for the interface or false if not</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="clsInterface"/> is null</exception>
        /// <exception cref="Exceptions.InterfaceNotFoundException">Thrown if the full name for <paramref name="clsInterface"/> cannot be found</exception>
        static public bool IsRegistered(Type clsInterface)
        {
            if (ReferenceEquals(clsInterface, null)) throw new ArgumentNullException(nameof(clsInterface));

            var ifFullName = clsInterface.FullName;

            if (String.IsNullOrEmpty(ifFullName)) throw new Exceptions.InterfaceNotFoundException(clsInterface.Name);

            return Configuration.InterfaceToImplementationMap.ContainsKey(ifFullName);
        }

        /// <summary>
        /// Creates a new instance of the configured implementing class for the given interface <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The interface for which an instance of the implementing class should be created</typeparam>
        /// <param name="args">Arguments for the constructor</param>
        /// <returns>A new instance of the implemented class wrapped in an <see cref="AspectProxy{T}"/></returns>
        /// <exception cref="Exceptions.InterfaceNotFoundException">Thrown if there is no implementing class configured for <typeparamref name="T"/></exception>
        /// <exception cref="V1.Exceptions.InfrastructureException">Thrown if an exception occurred while creating the instance of the implementing class</exception>
        static public T CreateInstance<T>(params object[] args) where T : class
        {
            var interfaceType = typeof(T);
            var interfaceFullName = interfaceType.FullName;

            if (!Configuration.InterfaceToImplementationMap.TryGetValue(interfaceFullName, out var implementingClassName)) implementingClassName = null;
            if (implementingClassName == null) throw new Exceptions.InterfaceNotFoundException(interfaceFullName);

            try
            {
                var implementingType = getTypeFromAssemblyForClass(implementingClassName);
                var implementingObj = Activator.CreateInstance(implementingType, args);
                var implementingClass = Convert.ChangeType(implementingObj, implementingType);
                var proxiedClass = AspectProxy<T>.Create((T)implementingClass, interfaceType) as T;

                return proxiedClass;
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

        /// <summary>
        /// Registers a given instance of a class (see <paramref name="instance"/>) to be returned by
        /// <see cref="ObjectFactory.CreateInstance{T}(object[])"/> for a certain type, therefore turning the
        /// type into a singleton.
        /// If an instance for a given type was already registered, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type to register <paramref name="instance"/> for</typeparam>
        /// <param name="instance">The instance to return when creation of a new instance for given <typeparamref name="T"/> is requested</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null</exception>
        static public void RegisterSingleton<T>(T instance)
        {
            if (ReferenceEquals(instance, null)) throw new ArgumentNullException(nameof(instance));

            var type = typeof(T);

            Configuration.InterfaceToInstanceMap[type] = instance;
        }

        /// <summary>
        /// Gets the instance that was registered as singleton for the given type via <see cref="RegisterSingleton{T}(T)"/>.
        /// </summary>
        /// <typeparam name="T">The type to get the singleton instance for</typeparam>
        /// <returns>The instance of the singleton or the default value for <typeparamref name="T"/> (which can be null) if no instance was registered</returns>
        static public T? GetInstance<T>() where T : class
        {
            var type = typeof(T);

            return GetInstance<T>(typeof(T));
        }

        /// <summary>
        /// Gets the instance that was registered as singleton for the given type <paramref name="interfaceType"/> via
        /// <see cref="RegisterSingleton{T}(T)"/> as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to cast the singleton instance to</typeparam>
        /// <param name="interfaceType">The type to get the singleton instance for</param>
        /// <returns>The instance of the singleton or the default value for <typeparamref name="T"/> (which can be null) if no instance was registered</returns>
        static public T? GetInstance<T>(Type interfaceType)
        {
            if (Configuration.InterfaceToInstanceMap.ContainsKey(interfaceType)) return (T)Configuration.InterfaceToInstanceMap[interfaceType];
            else return default;
        }
    }
}