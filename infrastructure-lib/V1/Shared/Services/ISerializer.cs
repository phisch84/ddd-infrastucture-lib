using System;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Services
{
    using Remoting.Models;

    /// <summary>
    /// Interface to the implementation of a serializer that is used to serialize data,
    /// which will be used by other services in the infrastructure layer.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes a given instance of an object into an array of bytes, so it can be either stored or transmitted.
        /// </summary>
        /// <typeparam name="T">The type of the instance to serialize</typeparam>
        /// <param name="data">The instance to serialize</param>
        /// <returns>The bytes representing <paramref name="data"/></returns>
        /// <remarks>This is the inverse operation of <see cref="Deserialize{T}(byte[])"/></remarks>
        byte[] Serialize<T>(T data);

        /// <summary>
        /// Serializes a given instance of any object assuming it is serializable as <paramref name="type"/>,
        /// so it can be either stored or transmitted
        /// </summary>
        /// <param name="data">The instance to serialize</param>
        /// <param name="type">The type to assume for serialization</param>
        /// <returns>The bytes representing <paramref name="data"/></returns>
        /// <remarks>This is the inverse operation of <see cref="Deserialize(byte[], Type)"/></remarks>
        byte[] Serialize(object data, Type type);

        /// <summary>
        /// Serializes an array of any object instances into a data model, that stores the original types of these objects,
        /// so it can be either stored or transmitted
        /// </summary>
        /// <param name="objects">The instances to serialize</param>
        /// <returns>A data model that can be deserialized later to copies of these objects</returns>
        DataTransferObject Serialize(object?[] objects);

        /// <summary>
        /// Deserializes the data of an object back into an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize into</typeparam>
        /// <param name="data">The data of the object to deserialze</param>
        /// <returns>An instance to the deserialized object</returns>
        /// <remarks>This is the inverse operation of <see cref="Serialize{T}(T)"/></remarks>
        T Deserialize<T>(byte[] data);

        /// <summary>
        /// Deserializes the data of an object back into an object.
        /// </summary>
        /// <param name="data">The data of the object to deserialze</param>
        /// <param name="type">The type of the object to deserialize into</param>
        /// <returns>An instance to the deserialized object</returns>
        /// <remarks>This is the inverse operation of <see cref="Serialize(object, Type)"/></remarks>
        object Deserialize(byte[] data, Type type);

        /// <summary>
        /// Deserializes a <see cref="DataTransferObject"/> back into the object(s) it was created from.
        /// </summary>
        /// <param name="dto">The <see cref="DataTransferObject"/> representing the serialized object(s)</param>
        /// <returns>
        /// An instance to the deserialized object(s) array.
        /// If only one object was serialized, the array will only have one element.
        /// If <see cref="null"/> or an empty array was serialized, the array will have no elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is null</exception>
        /// <exception cref="Exceptions.ClassNotRegisteredException">Thrown if <see cref="DataTransferObject.ClassId"/> in <paramref name="dto"/> is <see cref="Guid.Empty"/> or no class was found for <see cref="DataTransferObject.ClassId"/></exception>"
        /// <remarks>This is the inverse operation of <see cref="Serialize(object[])"/></remarks>
        object[] Deserialize(DataTransferObject dto);

        /// <summary>
        /// Registers a given <see cref="Guid"/> for a given <see cref="Type"/> which will then be used to serialize and
        /// deserialize <see cref="DataTransferObject"/>. The registration will be used in <see cref="Serialize(object[])"/>
        /// and <see cref="Deserialize(DataTransferObject)"/>.
        /// </summary>
        /// <param name="guidToRegister"></param>
        /// <param name="registrationForType"></param>
        void RegisterGuidForType(Guid guidToRegister, Type registrationForType);

        /// <summary>
        /// Looks up the <see cref="Type"/> for a given <paramref name="forClassId"/>
        /// and returns it in <paramref name="type"/>.
        /// </summary>
        /// <param name="forClassId">The <see cref="Type.GUID"/> to look up</param>
        /// <param name="type">The resulting <see cref="Type"/> if found.</param>
        /// <returns>
        /// True if the type was found for <paramref name="classId"/>, false otherwise
        /// </returns>
        bool TryLookUpType(Guid forClassId, out Type? type);

        /// <summary>
        /// Looks up the GUID for a given <paramref name="forType"/> and returns it in <paramref name="guid"/>.
        /// </summary>
        /// <param name="forType">The <see cref="Type"/> to get the GUID for</param>
        /// <param name="guid">The resulting <see cref="Guid"/> if found</param>
        /// <returns>
        /// True if the GUID was found for <paramref name="forType"/>, false otherwise
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forType"/> is null</exception>
        bool TryLookUpTypeGuid(Type forType, out Guid? guid);
    }
}
