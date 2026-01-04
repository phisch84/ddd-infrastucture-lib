using System;

namespace com.schoste.ddd.Infrastructure.V1.Remoting.Models
{
    /// <summary>
    /// Data transfer model for invocations of a remote method
    /// </summary>
    [Serializable]
    public class RemoteInvocation
    {
        /// <summary>
        /// The <see cref="Type.GUID"/> of the interface to the remote singleton which implements the method <see cref="TargetMethodName"/>
        /// </summary>
        public Guid TargetInterfaceTypeGUID { get; set; }

        /// <summary>
        /// The name of the method at the remote singleton to invoke
        /// </summary>
        public string? TargetMethodName { get; set; }

        /// <summary>
        /// Arguments encoded as <see cref="DataTransferObject"/> that match the signature of <see cref="TargetMethodName"/>
        /// </summary>
        public DataTransferObject? MethodArguments { get; set; }

        /// <summary>
        /// The return value of <see cref="TargetMethodName"/> after invoking it on the remote singleton
        /// </summary>
        public DataTransferObject? ReturnValue { get; set; }

        /// <summary>
        /// The exception thrown when invoking <see cref="TargetMethodName"/> on the remote singleton.
        /// </summary>
        public DataTransferObject? ThrownException { get; set; }
    }
}
