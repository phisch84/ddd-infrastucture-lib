using System;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Mocked
{
    using Models;
    using Shared.Services;

    public class LocalServer : Remoting.RemotingServer
    {
        public LocalServer() : base(ObjectFactory.CreateInstance<ISerializer>())
        {
        }

        virtual new public DataTransferObject ProcessMessage(long msgId, byte[] msg)
        {
            return base.ProcessMessage(msgId, msg).Result;
        }

        protected override long ReadNextMessage(out byte[] msg)
        {
            throw new NotImplementedException();
        }

        protected override Task SendResponse(long msgId, DataTransferObject response)
        {
            throw new NotImplementedException();
        }
    }
}
