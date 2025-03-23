using System;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Models
{
    /// <summary>
    /// Model for arguments in the event data are loaded for a <see cref="DataObject{ID}"/>.
    /// </summary>
    /// <typeparam name="DO">The actual type of the data object class</typeparam>
    /// <typeparam name="ID">The actual type of the data object's identifier</typeparam>
    public class LoadEventArgs<DO, ID> : EventArgs where DO : DataObject<ID>
    {
        /// <summary>
        /// Gets the unique id of the loading event.
        /// The id reported in <see cref="Services.IDataAccessObject{DO, ID}.BeforeLoad"/>
        /// corresponds to the one in <see cref="Services.IDataAccessObject{DO, ID}.AfterLoad"/>
        /// for one and the same loading event.
        /// </summary>
        public long CallId { get; protected set; }

        /// <summary>
        /// Gets the data object for which the data are loaded
        /// </summary>
        public DO DataObject { get; protected set; }

        /// <summary>
        /// Gets optional arguments received by <see cref="DataObject{ID}.LoadAsync(object[])"/>.
        /// </summary>
        public object[]? Arguments { get; protected set; }

        /// <summary>
        /// Creates an instance of this event argument for a specific load-event.
        /// </summary>
        /// <param name="callId">Sets <see cref="CallId"/>. Identifies the load-event</param>
        /// <param name="dataObject">Sets <see cref="DataObject"/>. The data object for which data are loaded</param>
        /// <param name="args">Sets <see cref="Arguments"/>. Pptional arguments received by <see cref="DataObject{ID}.LoadAsync(object[])"/></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataObject"/> is null</exception>
        public LoadEventArgs(long callId, DO dataObject, params object[] args) : base()
        {
            if (ReferenceEquals(dataObject, null)) throw new ArgumentNullException(nameof(dataObject));

            this.CallId = callId;
            this.DataObject = dataObject;
            this.Arguments = args;
        }
    }
}
