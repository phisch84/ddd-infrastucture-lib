using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// Make internals visible to the ProxyBuilder so instances will work when created via ObjectFactory
// and being decorated by the AspectProxy
[assembly: InternalsVisibleTo("ProxyBuilder")]

namespace com.schoste.ddd.Infrastructure.V1.DAL.Services
{
    using V1.DAL.Exceptions;
    using V1.DAL.Models;

    /// <summary>
    /// V1 abstract implementation of a generic Data Access Object (DAO) class.
    /// Implements <see cref="IDataAccessObject"/> and <see cref="IDataAccessObject{DO, ID}"/>.
    /// Concrete implementations of a DAO should extend this class.
    /// </summary>
    /// <typeparam name="DO">The actual type of the data object that is managed by the DAO</typeparam>
    /// <typeparam name="ID">The actual type of the data object's id</typeparam>
    abstract public class DataAccessObject<DO, ID> : IDataAccessObject, IDataAccessObject<DO, ID> where DO : DataObject<ID>
    {
        #region Internals
        void IDataAccessObject.Save(IEnumerable<DataObject> dataObjects)
        {
            try
            {
                this.Save(dataObjects.Cast<DO>());
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        void IDataAccessObject.Delete(IEnumerable<DataObject> dataObjects)
        {
            try
            {
                this.Delete(dataObjects.Cast<DO>());
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }
        #endregion

        /// <summary>
        /// Gets the instance of this DAO as <see cref="IDataAccessObject"/>
        /// </summary>
        virtual public IDataAccessObject GeneralInterface
        {
            get { return this; }
        }

        #region Load()
        /// <inheritdoc/>
        public event EventHandler<LoadEventArgs<DO, ID>>? BeforeLoad;

        /// <inheritdoc/>
        public event EventHandler<LoadEventArgs<DO, ID>>? AfterLoad;

        abstract protected object? LoadInternal(params object[] args);

        virtual protected void NotifyBeforeLoad(long callId, DO dataObject, params object[] args)
        {
            var eh = this.BeforeLoad;

            eh?.Invoke(this, new LoadEventArgs<DO, ID>(callId, dataObject, args));
        }

        virtual protected void NotifyAfterLoad(long callId, DO dataObject, params object[] args)
        {
            var eh = this.AfterLoad;

            eh?.Invoke(this, new LoadEventArgs<DO, ID>(callId, dataObject, args));
        }

        async Task<object?> IDataAccessObject.LoadAsync(object sender, params object[] args)
        {
            try
            {
                var callId = DateTime.Now.Ticks;

                this.NotifyBeforeLoad(callId, (DO)sender, args);

                var task = Task<object>.Run(() => { return this.LoadInternal(args); });

                await task.ContinueWith(t => { this.NotifyAfterLoad(callId, (DO)sender, [task.GetAwaiter().GetResult()]); });

                return task.Result;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }
        #endregion

        #region Create()
        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeCreateDataObject;

        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? AfterCreateDataObject;

        abstract protected DO CreateInternal(ID id, params object[] args);

        virtual protected void NotifyBeforeCreateDataObject(ID id)
        {
            var eh = this.BeforeCreateDataObject;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(id));
        }

        virtual protected void NotifyAfterCreateDataObject(DO dataObject)
        {
            var eh = this.AfterCreateDataObject;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObject));
        }

        /// <inheritdoc/>
        virtual public DO Create(ID id, params object[] args)
        {
            if (ReferenceEquals(id, null)) throw new ArgumentNullException(nameof(id));

            try
            {
                this.NotifyBeforeCreateDataObject(id);

                var dataObject = this.CreateInternal(id, args);
                    dataObject.DataAccessObject = this;
                    dataObject.Created = DateTime.Now;
                    dataObject.Modified = DateTime.Now;

                this.NotifyAfterCreateDataObject(dataObject);

                return dataObject;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual async public Task<DO> CreateAsync(ID id, params object[] args)
        {
            if (ReferenceEquals(id, null)) throw new ArgumentNullException(nameof(id));

            try
            {
                this.NotifyBeforeCreateDataObject(id);

                var task = Task<DO>.Run(() => 
                { 
                    var dataObj = this.CreateInternal(id, args);
                        dataObj.DataAccessObject = this;
                        dataObj.Created = DateTime.Now;

                    return dataObj;
                });

                await task.ContinueWith(t => { t.Result.Modified = DateTime.Now; this.NotifyAfterCreateDataObject(t.Result); });

                return task.Result;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }
        #endregion

        #region Save()
        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeSaveDataObjects;

        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? AfterSaveDataObjects;

        abstract protected void SaveInternal(IEnumerable<DO> dataObjects);

        virtual protected void NotifyBeforeSaveDataObjects(IEnumerable<DO> dataObjects)
        {
            var eh = this.BeforeSaveDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObjects));
        }

        virtual protected void NotifyAfterSaveDataObjects(IEnumerable<DO> dataObjects)
        {
            var eh = this.AfterSaveDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObjects));
        }

        /// <inheritdoc/>
        virtual public void Save(DO dataObject)
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            try
            {
                var dataObjects = new List<DO>(1) { dataObject };

                this.NotifyBeforeSaveDataObjects(dataObjects);
                this.SaveInternal(dataObjects);
                this.NotifyAfterSaveDataObjects(dataObjects);
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public void Save(IEnumerable<DO> dataObjects)
        {
            if (ReferenceEquals(dataObjects, null)) throw new ArgumentNullException(nameof(dataObjects));

            try
            {
                this.NotifyBeforeSaveDataObjects(dataObjects);
                this.SaveInternal(dataObjects);
                this.NotifyAfterSaveDataObjects(dataObjects);
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public async Task SaveAsync(DO dataObject)
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            try
            {
                var dataObjects = new List<DO>(1) { dataObject };

                this.NotifyBeforeSaveDataObjects(dataObjects);

                var task = Task.Run(() => this.SaveInternal(dataObjects));

                await task.ContinueWith(notifier => { this.NotifyAfterSaveDataObjects(dataObjects); });
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public async Task SaveAsync(IEnumerable<DO> dataObjects)
        {
            if (ReferenceEquals(dataObjects, null)) throw new ArgumentNullException(nameof(dataObjects));

            try
            {
                this.NotifyBeforeSaveDataObjects(dataObjects);

                var task = Task.Run(() => { this.SaveInternal(dataObjects); });
                
                await task.ContinueWith(notifier => { this.NotifyAfterSaveDataObjects(dataObjects); });
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }
        #endregion

        #region Get()
        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeGetDataObjects;

        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? AfterGetDataObjects;

        abstract protected IEnumerable<DO> GetInternal(IEnumerable<ID>? ids);

        virtual protected void NotifyBeforeGetDataObjects(IEnumerable<ID> ids)
        {
            var eh = this.BeforeGetDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(ids));
        }

        virtual protected void NotifyAfterGetDataObjects(IEnumerable<DO> dataObjects)
        {
            var eh = this.AfterGetDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObjects));
        }

        /// <inheritdoc/>
        virtual public IEnumerable<DO> Get()
        {
            try
            {
                this.NotifyBeforeGetDataObjects(new List<ID>(0));

                var dataObjects = this.GetInternal(null);
                
                this.NotifyAfterGetDataObjects(dataObjects);

                return dataObjects;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public IEnumerable<DO> Get(IEnumerable<ID> ids)
        {
            if (ReferenceEquals(ids, null)) throw new ArgumentNullException(nameof(ids));

            try
            {
                this.NotifyBeforeGetDataObjects(ids);

                var dataObjects = this.GetInternal(ids);

                this.NotifyAfterGetDataObjects(dataObjects);

                return dataObjects;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual async public Task<IEnumerable<DO>> GetAsync()
        {
            try
            {
                this.NotifyBeforeGetDataObjects(new List<ID>(0));

                var task = Task<IEnumerable<DO>>.Run(() => { return this.GetInternal(null); });
                
                await task.ContinueWith(t => { this.NotifyAfterGetDataObjects(t.Result); });

                return task.Result;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual async public Task<IEnumerable<DO>> GetAsync(IEnumerable<ID> ids)
        {
            if (ReferenceEquals(ids, null)) throw new ArgumentNullException(nameof(ids));

            try
            {
                this.NotifyBeforeGetDataObjects(ids);

                var task = Task<IEnumerable<DO>>.Run(() => { return this.GetInternal(ids); });

                await task.ContinueWith(t => { this.NotifyAfterGetDataObjects(t.Result); });

                return task.Result;
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        #endregion

        #region Delete()
        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? BeforeDeleteDataObjects;

        /// <inheritdoc/>
        public event EventHandler<DataObjectEventArgs<DO, ID>>? AfterDeleteDataObjects;

        abstract protected void DeleteInternal(IEnumerable<DO> dataObjects);

        virtual protected void NotifyBeforeDeleteDataObjects(IEnumerable<DO> dataObjects)
        {
            var eh = this.BeforeDeleteDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObjects));
        }

        virtual protected void NotifyAfterDeleteDataObjects(IEnumerable<ID> dataObjectIds)
        {
            var eh = this.AfterDeleteDataObjects;

            eh?.Invoke(this, new DataObjectEventArgs<DO, ID>(dataObjectIds));
        }

        /// <inheritdoc/>
        virtual public void Delete(DO dataObject)
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            try
            {
                var dataObjId = dataObject.Id;
                var dataObjects = new List<DO>(1) { dataObject };

                this.NotifyBeforeDeleteDataObjects(dataObjects);
                this.DeleteInternal(dataObjects);
                this.NotifyAfterDeleteDataObjects(new List<ID>(1) { dataObjId });
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public void Delete(IEnumerable<DO> dataObjects)
        {
            if (ReferenceEquals(dataObjects, null)) throw new ArgumentNullException(nameof(dataObjects));

            try
            {
                var dataObjIds = dataObjects.Select(dataObj => dataObj.Id);

                this.NotifyBeforeDeleteDataObjects(dataObjects);
                this.DeleteInternal(dataObjects);
                this.NotifyAfterDeleteDataObjects(dataObjIds);
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public async Task DeleteAsync(DO dataObject)
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            try
            {
                var dataObjId = dataObject.Id;
                var dataObjects = new List<DO>(1) { dataObject };

                this.NotifyBeforeDeleteDataObjects(dataObjects);

                var task = Task.Run(() => { this.DeleteInternal(new List<DO>(1) { dataObject }); });

                await task.ContinueWith(notifier => { this.NotifyAfterDeleteDataObjects(new List<ID>(1) { dataObjId }); });
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }

        /// <inheritdoc/>
        virtual public async Task DeleteAsync(IEnumerable<DO> dataObjects)
        {
            if (ReferenceEquals(dataObjects, null)) throw new ArgumentNullException(nameof(dataObjects));

            try
            {
                var dataObjIds = dataObjects.Select(dataObj => dataObj.Id);

                this.NotifyBeforeDeleteDataObjects(dataObjects);

                var task = Task.Run(() => { this.DeleteInternal(dataObjects); });
                
                await task.ContinueWith(notifier => { this.NotifyAfterDeleteDataObjects(dataObjIds); });
            }
            catch (V1.Exceptions.InfrastructureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DALException(ex);
            }
        }
        #endregion
    }
}
