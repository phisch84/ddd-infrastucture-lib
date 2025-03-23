using System;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Stream
{
    using Shared.Services;

    /// <summary>
    /// Basis extensions of the <see cref="Remoting.RemotingClient"/> abstract class for communications that
    /// can be organized using <see cref="System.IO.Stream"/>
    /// </summary>
    public class RemotingClient : Remoting.RemotingClient
    {
        protected System.IO.Stream InputStream;
        protected System.IO.Stream OutputStream;

        public RemotingClient(ISerializer serializer, System.IO.Stream inputStream, System.IO.Stream outputStream) : base(serializer)
        {
            this.Serializer = serializer;
            this.InputStream = inputStream;
            this.OutputStream = outputStream;
        }

        override protected async Task SendInvocationAsync(long msgId, byte[] invocation)
        {
            this.OutputStream.Write(BitConverter.GetBytes(msgId));
            this.OutputStream.Write(BitConverter.GetBytes(invocation.Length));
            this.OutputStream.Write(invocation);

            await this.OutputStream.FlushAsync();
        }

        override protected byte[] ReadNextResponse(out long msgId)
        {
            if (ReferenceEquals(this.InputStream, null)) throw new InvalidOperationException();

            var retMsgIdBuffer = new byte[sizeof(long)];
            var retValLenBuffer = new byte[sizeof(int)];

            this.InputStream.Read(retMsgIdBuffer, 0, retMsgIdBuffer.Length);
            this.InputStream.Read(retValLenBuffer, 0, retValLenBuffer.Length);

            msgId = BitConverter.ToInt64(retMsgIdBuffer);

            var retValLen = BitConverter.ToInt32(retValLenBuffer);
            var retValBuffer = new byte[retValLen];

            this.InputStream.Read(retValBuffer, 0, retValLen);

            return retValBuffer;
        }
    }
}
