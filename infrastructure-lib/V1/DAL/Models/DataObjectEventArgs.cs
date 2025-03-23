using System;
using System.Collections.Generic;
using System.Linq;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Models
{
    /// <summary>
    /// Model for events of the <see cref="V1.DAL.Services.DataAccessObject{DO, ID}"/> class related to manipulations of
    /// <see cref="DataObject{ID}"/> classes.
    /// </summary>
    /// <typeparam name="DO">The actual class of the data object</typeparam>
    /// <typeparam name="ID">The class of the data object's id</typeparam>
    public class DataObjectEventArgs<DO, ID> : EventArgs where DO : DataObject<ID>
    {
        /// <summary>
        /// Gets the ids of the affected data objects
        /// </summary>
        virtual public IEnumerable<ID> Ids { get; protected set; }

        /// <summary>
        /// Gets the affected data objects.
        /// May be null if none were provided (e.g., only ids were provided).
        /// </summary>
        virtual public IEnumerable<DO>? DataObjects { get; protected set; }

        /// <summary>
        /// Creates an instance of the event for a given data object identified by its <see cref="DataObject{ID}.Id"/>
        /// </summary>
        /// <param name="id">The id of the data object for which this event is created</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null</exception>
        public DataObjectEventArgs(ID id) : base()
        {
            if (ReferenceEquals(id, null)) throw new ArgumentNullException(nameof(id));

            this.Ids = new List<ID>(1) { id };
        }

        /// <summary>
        /// Creates an instance of the event for given data objects identified by their <see cref="DataObject{ID}.Id"/>
        /// </summary>
        /// <param name="id">A set of ids of the data objects for which this event is created</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ids"/> is null</exception>
        public DataObjectEventArgs(IEnumerable<ID> ids) : base()
        {
            if (ReferenceEquals(ids, null)) throw new ArgumentNullException(nameof(ids));

            this.Ids = new List<ID>(ids);
        }

        /// <summary>
        /// Creates an instance of the event for a given data object
        /// </summary>
        /// <param name="dataObject">The data object for which this event is created</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null</exception>
        public DataObjectEventArgs(DO dataObject) : base()
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            this.Ids = new List<ID>(1) { dataObject.Id };
            this.DataObjects = new List<DO>(1) { dataObject };
        }

        /// <summary>
        /// Creates an instance of the event for given data objects.
        /// </summary>
        /// <param name="id">A set of data objects for which this event is created</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObjects"/> is null</exception>
        public DataObjectEventArgs(IEnumerable<DO> dataObjects) : base()
        {
            if (ReferenceEquals(dataObjects, null)) throw new ArgumentNullException(nameof(dataObjects));

            this.Ids = new List<ID>(dataObjects.Select(dataObj => dataObj.Id));
            this.DataObjects = new List<DO>(dataObjects);
        }
    }
}
