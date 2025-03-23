using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Services
{
    using V1.DAL.Models;

    /// <summary>
    /// Public general interface of the <see cref="DataAccessObject{DO, ID}"/> class.
    /// </summary>
    public interface IDataAccessObject
    {
        /// <summary>
        /// Gets the general interface to the <see cref="DataAccessObject{DO, ID}"/>
        /// </summary>
        public IDataAccessObject GeneralInterface { get; }

        /// <summary>
        /// Synchronously saves data objects to the underlying storage
        /// </summary>
        /// <param name="dataObject">The data objects to save</param>
        void Save(IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Synchronously deletes data objects from the underlying storage
        /// </summary>
        /// <param name="dataObjects">The data objects to delete</param>
        void Delete(IEnumerable<DataObject> dataObjects);

        /// <summary>
        /// Called by <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> when additional
        /// data shall be loaded
        /// </summary>
        /// <param name="sender">The calling data object</param>
        /// <param name="args">Optional arguments</param>
        /// <returns>A task that loads the data</returns>
        internal Task<object> LoadAsync(object sender, params object[] args);
    }

    /// <summary>
    /// Version 1 public interface to the abstract <see cref="DataAccessObject{DO, ID}"/>.
    /// To be extended by interfaces to data access objects that derive from the <see cref="DataAccessObject{DO, ID}"/>.
    /// </summary>
    /// <typeparam name="DO">The actual type of the data object managed by this data access object</typeparam>
    /// <typeparam name="ID">The actual type of the data object's identifier</typeparam>
    public interface IDataAccessObject<DO, ID> : IDataAccessObject where DO : DataObject<ID>
    {
        /// <summary>
        /// Called before a new data object is created.
        /// (before <see cref="DataAccessObject{DO, ID}.CreateInternal(DO, object[])"/> is called).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>> BeforeCreateDataObject;

        /// <summary>
        /// Called after a new data object was created.
        /// (after <see cref="DataAccessObject{DO, ID}.CreateInternal(DO, object[])"/> returned successfully).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>> AfterCreateDataObject;

        /// <summary>
        /// Synchronously creates a new instance of a data object
        /// </summary>
        /// <param name="id">The identifier of the new data object</param>
        /// <param name="args">
        /// Optional arguments for the creation.
        /// Their application depend on the extension/implementation of this method
        /// </param>
        /// <returns>The instance of a new data object</returns>
        DO Create(ID id, params object[] args);

        /// <summary>
        /// Asynchronously creates a new instance of a data object
        /// </summary>
        /// <param name="id">The identifier of the new data object</param>
        /// <param name="args">
        /// Optional arguments for the creation.
        /// Their application depend on the extension/implementation of this method
        /// </param>
        /// <returns>The task that creates an instance of a new data object</returns>
        Task<DO> CreateAsync(ID id, params object[] args);

        /// <summary>
        /// Called before additional data for a data object are loaded.
        /// (before <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> is called).
        /// </summary>
        event EventHandler<LoadEventArgs<DO, ID>> BeforeLoad;

        /// <summary>
        /// Called after additional data for a data object were loaded.
        /// /after the task of <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> completed successfully).
        /// </summary>
        event EventHandler<LoadEventArgs<DO, ID>> AfterLoad;

        /// <summary>
        /// Called before data objects are saved to the underlying storage.
        /// (before <see cref="DataAccessObject{DO, ID}.SaveInternal(IEnumerable{DO})"/> is called).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>> BeforeSaveDataObjects;

        /// <summary>
        /// Called after data objects were successfully saved to the underlying storage.
        /// (after the task of <see cref="DataAccessObject{DO, ID}.SaveInternal(IEnumerable{DO})"/> completed successfully).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>> AfterSaveDataObjects;

        /// <summary>
        /// Synchronously saves a data object to the underlying storage
        /// </summary>
        /// <param name="dataObject">The data object to save</param>
        void Save(DO dataObject);

        /// <summary>
        /// Synchronously saves data objects to the underlying storage
        /// </summary>
        /// <param name="dataObject">The data objects to save</param>
        void Save(IEnumerable<DO> dataObjects);

        /// <summary>
        /// Asynchronously saves a data object to the underlying storage
        /// </summary>
        /// <param name="dataObject">The data object to save</param>
        /// <returns>A task that performs the save operation</returns>
        Task SaveAsync(DO dataObject);

        /// <summary>
        /// Asynchronously saves data objects to the underlying storage
        /// </summary>
        /// <param name="dataObjects">The data objects to save</param>
        /// <returns>A task that performs the save operation</returns>
        Task SaveAsync(IEnumerable<DO> dataObjects);

        /// <summary>
        /// Called before data objects were loaded from the underlying storage.
        /// (before <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> is called).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeGetDataObjects;

        /// <summary>
        /// Called after data objects were loaded from the underlying storage.
        /// (after the task of <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> completed successfully).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>>? AfterGetDataObjects;

        /// <summary>
        /// Synchronously gets all available data objects from the underlying storage
        /// </summary>
        /// <returns>All available data objects</returns>
        IEnumerable<DO> Get();

        /// <summary>
        /// Synchronously gets all data objects from the underlying storage with the given ids
        /// </summary>
        /// <param name="ids">The ids of the data objects to load</param>
        /// <returns>
        /// A collection of data objects.
        /// It's up to the caller to determine if all objects were loaded or not.
        /// </returns>
        IEnumerable<DO> Get(IEnumerable<ID> ids);

        /// <summary>
        /// Asynchronously gets all available data objects from the underlying storage
        /// </summary>
        /// <returns>A taks that loads all available data objects</returns>
        Task<IEnumerable<DO>> GetAsync();

        /// <summary>
        /// Asynchronously gets all data objects from the underlying storage with the given ids
        /// </summary>
        /// <param name="ids">The ids of the data objects to load</param>
        /// <returns>
        /// A taks that loads data objects and returns a collection of data objects as result.
        /// It's up to the caller to determine if all objects were loaded or not.
        /// </returns>
        Task<IEnumerable<DO>> GetAsync(IEnumerable<ID> ids);

        /// <summary>
        /// Called before data objects are deleted from the underlying storage.
        /// (before <see cref="DataAccessObject{DO, ID}.DeleteInternal(IEnumerable{DO})"/> is called).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeDeleteDataObjects;

        /// <summary>
        /// Called after data objects were successfully deleted from the underlying storage.
        /// (after the task of <see cref="DataAccessObject{DO, ID}.DeleteInternal(IEnumerable{DO})"/> completed successfully).
        /// </summary>
        event EventHandler<DataObjectEventArgs<DO, ID>>? AfterDeleteDataObjects;

        /// <summary>
        /// Synchronously deletes a data object from the underlying storage
        /// </summary>
        /// <param name="dataObject">The data object to delete</param>
        void Delete(DO dataObject);

        /// <summary>
        /// Synchronously deletes data objects from the underlying storage
        /// </summary>
        /// <param name="dataObjects">The data objects to delete</param>
        void Delete(IEnumerable<DO> dataObjects);

        /// <summary>
        /// Asynchronously deletes a data object from the underlying storage
        /// </summary>
        /// <param name="dataObject">The data object to delete</param>
        /// <returns>A task that performs the delete operation</returns>
        Task DeleteAsync(DO dataObject);

        /// <summary>
        /// Asynchronously deletes data objects from the underlying storage
        /// </summary>
        /// <param name="dataObjects">The data objects to delete</param>
        /// <returns>A task that performs the delete operation</returns>
        Task DeleteAsync(IEnumerable<DO> dataObjects);
    }
}
