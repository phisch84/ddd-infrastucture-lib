using System;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Models
{
    using V1.DAL.Services;

    /// <summary>
    /// Provides type-agnostic methods and properties for <see cref="DataObject{ID}"/>.
    /// </summary>
    abstract public class DataObject
    {
        internal IDataAccessObject? DataAccessObject;
    }

    /// <summary>
    /// Basic data object class which is used in <see cref="IDataAccessObject{DO, ID}"/>.
    /// </summary>
    /// <typeparam name="ID">The data type of the data object's identifier</typeparam>
    [Serializable]
    abstract public class DataObject<ID> : DataObject
    {
        /// <summary>
        /// Gets or sets the identifier of the data object.
        /// </summary>
        public ID Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the data object was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the data object was last modified
        /// </summary>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Creates a new instance of the data object with a given id
        /// </summary>
        /// <param name="id">The id of the new instance</param>
        public DataObject(ID id)
        {
            this.Id = id;
        }

        /// <summary>
        /// To be called by deriving classes to load further data from storage.
        /// E.g., if lazy loading shall be implemented
        /// </summary>
        /// <param name="args">Optional arguments to be passed on to <see cref="IIDataAccessObject{DO, ID}.LoadAsync(object, object[])"/></param>
        /// <returns>A task that tries to load the data</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="DataAccessObject"/> is null</exception>
        protected async Task<object> LoadAsync(params object[] args)
        {
            if (!ReferenceEquals(DataAccessObject, null)) return await this.DataAccessObject.LoadAsync(this, args);
            else throw new InvalidOperationException();
        }
    }
}
