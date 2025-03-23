using System;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    public interface IRemotingServer : IDisposable
    {
        /// <summary>
        /// Gets or sets the configuration of the remoting server.
        /// What the actual configuration looks like, depends on the server implementation
        /// </summary>
        object? Configuration { get; set; }

        /// <summary>
        /// Runs the server so it starts listening for calls from clients.
        /// </summary>
        /// <returns>A task in which the server is running</returns>
        Task Start();

        /// <summary>
        /// Gracefully stops the server
        /// </summary>
        void Stop();
    }
}
