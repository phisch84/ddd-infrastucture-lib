using System;
using System.Collections.Generic;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Models
{
    /// <summary>
    /// V1 model of the <see cref="Services.ObjectFactory"/>'s configuration
    /// </summary>
    public class ObjectFactoryConfiguration
    {
        /// <summary>
        /// Defines which class shall be loaded for which interface when creating a new instance
        /// via <see cref="V1.Shared.Services.ObjectFactory.CreateInstance{T}(object[])"/>
        /// </summary>
        public IDictionary<string, string> InterfaceToImplementationMap { get; protected set; }

        /// <summary>
        /// Defines which instance shall be returned for which interface when creating a new instance
        /// via <see cref="V1.Shared.Services.ObjectFactory.CreateInstance{T}(object[])"/>.
        /// Since the same instance is always returned, this will turn the implementation of an interface
        /// into a singleton.
        /// </summary>
        public IDictionary<Type, object> InterfaceToInstanceMap { get; protected set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ObjectFactoryConfiguration()
        {
            this.InterfaceToImplementationMap = new Dictionary<string, string>();
            this.InterfaceToInstanceMap = new Dictionary<Type, object>();
        }
    }
}
