using System.Text.Json;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Stream.AnonPipes
{
    using Remoting.Models;
    using Shared.Services;

    /// <summary>
    /// Helper extension of the <see cref="RemotingClient"/> which works with a <see cref="JsonSerializer"/>.
    /// When serialized results arrive, it converts the <see cref="JsonElement"/> return-value into the actual
    /// value type.
    /// </summary>
    public class JsonRemotingClient : RemotingClient
    {
        public JsonRemotingClient(ISerializer serializer) : base(serializer)
        { }

        override protected RemoteInvocation ProcessResponse(byte[] responseMsg)
        {
            var response = base.ProcessResponse(responseMsg);
            var deserializedRetVal = !ReferenceEquals(null, response.ReturnValue) ? this.Serializer.Deserialize(response.ReturnValue) : null;
            var jsonRetValue = (ReferenceEquals(null, deserializedRetVal) || (deserializedRetVal.Length == 0)) ? null : deserializedRetVal[0] as JsonElement?;

            return response;
        }

        virtual protected object? CastJsonToType(JsonElement? value)
        {
            if (value == null) return null;

            switch (value.Value.ValueKind)
            {
                case JsonValueKind.Null: return null;
                case JsonValueKind.String: return value.Value.ToString();
                default: return null;
            }
        }
    }
}
