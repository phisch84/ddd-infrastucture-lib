using System;
using System.IO.Pipes;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Stream.AnonPipes
{
    using Shared.Services;

    /// <summary>
    /// Implementation of the <see cref="Stream.RemotingServer"/> base class that uses anonymous pipes for an insecure,
    /// inter-process communication.
    /// </summary>
    public class RemotingServer : Stream.RemotingServer, IDisposable
    {
        protected AnonymousPipeClientStream? InputStreamClient;
        protected AnonymousPipeClientStream? OutputStreamClient;

        /// <summary>
        /// Creates a new instance of the server
        /// </summary>
        /// <param name="serializer">Interface to the serializer that is used to (de)serialize transferred data</param>
        /// <param name="inputPipeHandle">
        /// The handle to the anonymous input pipe. This is typically <see cref="RemotingClient.OutputPipeClientHandle"/>
        /// </param>
        /// <param name="outputPipeHandle">
        /// The handle to the anonymous output pipe. This is typically <see cref="RemotingClient.InputPipeClientHandle"/>
        /// </param>
        public RemotingServer(ISerializer serializer, string inputPipeHandle, string outputPipeHandle) : base(serializer, new AnonymousPipeClientStream(PipeDirection.In, inputPipeHandle), new AnonymousPipeClientStream(PipeDirection.Out, outputPipeHandle))
        {
            this.InputStreamClient = (AnonymousPipeClientStream)base.InputStream;
            this.OutputStreamClient = (AnonymousPipeClientStream)base.OutputStream;
        }

        #region Dispose()
        private bool disposedValue;

        override protected void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposedValue)
            {
                if (disposing)
                {
                    this.InputStreamClient?.Dispose();
                    this.InputStreamClient = null;
                    this.OutputStreamClient?.Dispose();
                    this.OutputStreamClient = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
        #endregion
    }
}
