using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Models
{
    /// <summary>
    /// Model to serialize data with <see cref="Services.ISerializer"/> so they can be sent via <see cref="IRemotingClient"/>
    /// </summary>
    [Serializable]
    public class DataTransferObject
    {
        /// <summary>
        /// Gets or sets the unique id of the object's type that was serialized
        /// (see <see cref="Type.GUID"/>), so it can be deserialized back to the proper type.
        /// </summary>
        public Guid ClassId { get; set; }

        /// <summary>
        /// Gets or sets the serialized data of the object
        /// </summary>
        public byte[]? Data { get; set; }

        /// <summary>
        /// Default empty constructor used in serialization
        /// </summary>
        public DataTransferObject() { }

        /// <summary>
        /// Constructor to create an instance that holds a serialized object.
        /// </summary>
        /// <param name="classId">The <see cref="Type.GUID"/> of the class which's data are encoded in <paramref name="data"/></param>
        /// <param name="data">The binary data of the serialized object</param>
        public DataTransferObject(Guid classId, byte[] data)
        {
            ClassId = classId;
            Data = data;
        }
    }
}
