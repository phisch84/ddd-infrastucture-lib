using System;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Stream
{
    using Remoting.Models;
    using Shared.Services;

    /// <summary>
    /// Basis extensions of the <see cref="Remoting.RemotingServer"/> abstract class for communications that
    /// can be organized using <see cref="System.IO.Stream"/>
    /// </summary>
    public class RemotingServer : Remoting.RemotingServer
    {
        protected System.IO.Stream InputStream;
        protected System.IO.Stream OutputStream;

        public RemotingServer(ISerializer serializer, System.IO.Stream inputStream, System.IO.Stream outputStream) : base(serializer)
        {
            if (ReferenceEquals(null, inputStream)) throw new ArgumentNullException(nameof(inputStream));
            if (ReferenceEquals(null, outputStream)) throw new ArgumentNullException(nameof(outputStream));

            this.InputStream = inputStream;
            this.OutputStream = outputStream;
        }

        override protected long ReadNextMessage(out byte[] msg)
        {
            var bufferMsgId_sz = sizeof(long);
            var bufferMsgId = new byte[bufferMsgId_sz];
            var bufferSize_sz = sizeof(int);
            var bufferSize = new byte[bufferSize_sz];

            this.InputStream.Read(bufferMsgId, 0, bufferMsgId_sz);
            this.InputStream.Read(bufferSize, 0, bufferSize_sz);

            var msgId = BitConverter.ToInt64(bufferMsgId, 0);
            var size = BitConverter.ToInt32(bufferSize, 0);

            if (size > 0)
            {
                msg = new byte[size];

                this.InputStream.Read(msg, 0, size);
            }
            else
            {
                msg = Array.Empty<byte>();
            }

            return msgId;
        }

        override protected Task SendResponse(long msgId, DataTransferObject response)
        {
            var invocation = this.Serializer.Serialize(response);

            return Task.Run(() => { this.SendResponse(msgId, invocation); });
        }

        virtual protected void SendResponse(long msgId, byte[] invocation)
        {
            this.OutputStream.Write(BitConverter.GetBytes(msgId));
            this.OutputStream.Write(BitConverter.GetBytes(invocation.Length));
            this.OutputStream.Write(invocation);
            this.OutputStream.Flush();
        }
    }
}
