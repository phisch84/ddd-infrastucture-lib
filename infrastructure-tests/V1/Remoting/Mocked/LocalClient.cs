using System;
using System.Reflection;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Mocked
{
    using Models;
    using Shared.Services;

    public class LocalClient : Remoting.RemotingClient, IRemotingClient
    {
        public LocalServer LocalServer { get; protected set; }
        public long LastMsgId { get; protected set; }
        public DataTransferObject? LastInvocation { get; protected set; }

        public LocalClient(ISerializer serializer) : base(serializer)
        { 
            LocalServer = new LocalServer();
        }

        protected override Task SendInvocationAsync(long msgId, byte[] invocation)
        {
            this.LastMsgId = msgId;
            this.LastInvocation = this.LocalServer.ProcessMessage(msgId, invocation);

            return Task.CompletedTask;
        }

        public new byte[] SerializeInvocation(object?[]? args, Type interfaceType, MethodInfo remoteMethod)
        {
            return base.SerializeInvocation(args, interfaceType, remoteMethod);
        }

        protected override byte[] ReadNextResponse(out long msgId)
        {
            msgId = this.LastMsgId;

            var rspMsg = this.Serializer.Serialize(this.LastInvocation);

            return rspMsg;
        }
    }
}
