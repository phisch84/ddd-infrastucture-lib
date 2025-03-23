using System;
using System.IO.Pipes;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Mocked
{
    /// <summary>
    /// The stream a <see cref="RemotingServer"/> writes to and a <see cref="RemotingClient"/> reads from.
    /// </summary>
    public class ServerStream : IDisposable
    {
        private bool disposedValue;

        protected AnonymousPipeServerStream PipeServerStream;

        public ServerStream()
        {
            PipeServerStream = new AnonymousPipeServerStream(PipeDirection.In);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    PipeServerStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServerStream()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
