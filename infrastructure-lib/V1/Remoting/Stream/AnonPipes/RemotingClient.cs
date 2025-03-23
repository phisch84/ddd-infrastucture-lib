using System;
using System.IO;
using System.IO.Pipes;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Stream.AnonPipes
{
    using Shared.Services;

    /// <summary>
    /// Implementation of the <see cref="Stream.RemotingClient"/> base class that uses anonymous pipes for an insecure,
    /// inter-process communication.
    /// </summary>
    public class RemotingClient : Stream.RemotingClient, IRemotingClient, IDisposable
    {
        private bool disposedValue;

        protected AnonymousPipeServerStream InputStreamServer;
        protected AnonymousPipeServerStream OutputStreamServer;

        /// <summary>
        /// Gets the client handle of the input pipe's stream.
        /// This is typically the output stream for the <see cref="RemotingServer"/>.
        /// </summary>
        public string InputPipeClientHandle
        {
            get
            {
                return this.InputStreamServer.GetClientHandleAsString();
            }
        }

        /// <summary>
        /// Gets the client handle of the output pipe's stream.
        /// This is typically the input stream for the <see cref="RemotingServer"/>.
        /// </summary>
        public string OutputPipeClientHandle
        {
            get
            {
                return this.OutputStreamServer.GetClientHandleAsString();
            }
        }

        /// <summary>
        /// <para><inheritdoc/></para>
        /// <para>
        /// The configuration of this class is a <see cref="System.String[]"/> with <see cref="OutputPipeClientHandle"/>
        /// as first element and <see cref="InputPipeClientHandle"/> as second element.
        /// </para>
        /// </summary>
        override public object? Configuration 
        {
            get
            {
                return new string[] { this.OutputPipeClientHandle, this.InputPipeClientHandle };
            }
        }

        /// <summary>
        /// Creates a new instance of the client.
        /// The anonymous pipes will be created by the implementation.
        /// </summary>
        /// <param name="serializer">Interface to the serializer that is used to (de)serialize transferred data</param>
        public RemotingClient(ISerializer serializer) : base(serializer, new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable), new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
        {
            this.InputStreamServer = (AnonymousPipeServerStream)base.InputStream;
            this.OutputStreamServer = (AnonymousPipeServerStream)base.OutputStream;
        }

        #region Dispose()
        virtual protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.InputStreamServer?.DisposeLocalCopyOfClientHandle();
                    this.InputStreamServer?.Dispose();
                    this.OutputStreamServer?.DisposeLocalCopyOfClientHandle();
                    this.OutputStreamServer?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ClientStream()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        #endregion
    }
}
