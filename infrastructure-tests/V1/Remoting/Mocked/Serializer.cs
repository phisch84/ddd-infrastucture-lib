using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Mocked
{
    using Remoting.Models;
    using Shared.Exceptions;
    using Shared.Models;

    public class Serializer : Shared.Services.ISerializer
    {
        public readonly IDictionary<Guid, Type> ClsGuidsToTypesMap = new Dictionary<Guid, Type>();
        public readonly IDictionary<Type, Guid> TypesToClsGuidsMap = new Dictionary<Type, Guid>();

        public Serializer()
        {
        }

        /// <inheritdoc/>
        virtual public void RegisterGuidForType(Guid guidToRegister, Type registrationForType)
        {
            this.ClsGuidsToTypesMap[guidToRegister] = registrationForType;
            this.TypesToClsGuidsMap[registrationForType] = guidToRegister;
        }

        /// <inheritdoc/>
        virtual public bool TryLookUpType(Guid forClassId, out Type? type)
        {
            type = null;

            if (this.ClsGuidsToTypesMap.ContainsKey(forClassId))
            {
                type = this.ClsGuidsToTypesMap[forClassId];

                return true;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var typesOfAssemblies = assemblies.SelectMany(asm => asm.GetTypes());

            if (ReferenceEquals(typesOfAssemblies, null)) return false;

            type = typesOfAssemblies.Where(t => t != null).FirstOrDefault(t => t.GUID == forClassId);

            if (type == null) return false;
            
            this.ClsGuidsToTypesMap[forClassId] = type;

            return true;
        }

        /// <inheritdoc/>
        virtual public bool TryLookUpTypeGuid(Type forType, out Guid? guid)
        {
            guid = null;

            if (ReferenceEquals(null, forType)) throw new ArgumentNullException(nameof(forType));
            if (this.TypesToClsGuidsMap.ContainsKey(forType)) guid = this.TypesToClsGuidsMap[forType];

            guid = guid ?? forType.GUID;

            return (guid != null);
        }

        /// <inheritdoc/>
        virtual public T Deserialize<T>(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (data.Length == 0) return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        /// <inheritdoc/>
        virtual public object Deserialize(byte[] data, Type type)
        {
            ArgumentNullException.ThrowIfNull(data);

            return JsonSerializer.Deserialize(data, type);
        }

        /// <inheritdoc/>
        virtual public object[] Deserialize(DataTransferObject dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.Data == null) return new object[0];

            var dtos = this.Deserialize<DataTransferObject[]>(dto.Data);
            var objects = ReferenceEquals(null, dtos) ? new object[0] : new object[dtos.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                if (Guid.Empty.Equals(dtos[i].ClassId)) throw new ClassNotRegisteredException(String.Empty);
                if(!this.TryLookUpType(dtos[i].ClassId, out var type)) throw new ClassNotRegisteredException(dtos[i].ClassId.ToString());

                objects[i] = this.Deserialize(dtos[i].Data, type);
            }

            return objects;
        }

        /// <inheritdoc/>
        virtual public byte[] Serialize<T>(T data)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }

        /// <inheritdoc/>
        virtual public byte[] Serialize(object data, Type type)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, type));
        }

        /// <inheritdoc/>
        virtual public DataTransferObject Serialize(object?[] objects)
        {
            if (object.ReferenceEquals(objects, null)) throw new ArgumentNullException(nameof(objects));
            if (objects.Length == 0) return new DataTransferObject()
            {
                ClassId = typeof(void).GUID,
                Data = new byte[0],
            };

            var dtos = new DataTransferObject[objects.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                var type = objects[i].GetType();
                var typeFullName = type.FullName ?? String.Empty;
                var typeGuid = type.GUID as Guid?;

                if (!this.TryLookUpTypeGuid(type, out typeGuid)) throw new ClassNotRegisteredException(typeFullName);
                if ((typeGuid == null) || Guid.Empty.Equals(typeGuid)) throw new ClassNotRegisteredException(typeFullName);

                var data = this.Serialize(objects[i], type);
                var dto = new DataTransferObject((Guid)typeGuid, data);

                dtos[i] = dto;
            }

            var returnDTO = new DataTransferObject()
            {
                ClassId = typeof(DataTransferObject[]).GUID,
                Data = this.Serialize<DataTransferObject[]>(dtos),
            };

            return returnDTO;
        }
    }
}
