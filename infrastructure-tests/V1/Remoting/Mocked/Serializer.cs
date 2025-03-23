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
        protected readonly IDictionary<Guid, Type> ClsGuidsToTypesMap = new Dictionary<Guid, Type>();
        protected readonly IDictionary<Type, Guid> TypesToClsGuidsMap = new Dictionary<Type, Guid>();

        public Serializer()
        {
        }

        virtual public void RegisterGuidForType(Guid guidToRegister, Type registrationForType)
        {
            this.ClsGuidsToTypesMap[guidToRegister] = registrationForType;
            this.TypesToClsGuidsMap[registrationForType] = guidToRegister;
        }

        virtual public Type LookUpType(Guid classId)
        {
            if (this.ClsGuidsToTypesMap.ContainsKey(classId)) return this.ClsGuidsToTypesMap[classId];

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var typesOfAssemblies = assemblies.SelectMany(asm => asm.GetTypes());
            var type = typesOfAssemblies.Where(t => t != null).FirstOrDefault(t => t.GUID == classId);

            if (type == null) throw new ClassNotFoundException(classId.ToString());

            this.ClsGuidsToTypesMap[classId] = type;

            return type;
        }

        virtual public Guid LookUpTypeGuid(Type type)
        {
            if (ReferenceEquals(null, type)) throw new ArgumentNullException(nameof(type));
            if (this.TypesToClsGuidsMap.ContainsKey(type)) return this.TypesToClsGuidsMap[type];

            return type.GUID;
        }

        public T Deserialize<T>(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            if (data.Length == 0) return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public object Deserialize(byte[] data, Type type)
        {
            ArgumentNullException.ThrowIfNull(data);

            return JsonSerializer.Deserialize(data, type);
        }

        public object[] Deserialize(DataTransferObject dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.Data == null) return new object[0];

            var dtos = this.Deserialize<DataTransferObject[]>(dto.Data);
            var objects = ReferenceEquals(null, dtos) ? new object[0] : new object[dtos.Length];

            for (var i = 0; i < objects.Length; i++)
            {
                var type = this.LookUpType(dtos[i].ClassId);

                objects[i] = this.Deserialize(dtos[i].Data, type);
            }

            return objects;
        }

        public byte[] Serialize<T>(T data)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }

        public byte[] Serialize(object data, Type type)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, type));
        }

        public DataTransferObject Serialize(object[] objects)
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
                var data = this.Serialize(objects[i], type);
                var dto = new DataTransferObject(type.GUID, data);

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
