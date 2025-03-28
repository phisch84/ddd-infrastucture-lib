﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace com.schoste.ddd.Infrastructure.V1.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("com.schoste.ddd.Infrastructure.V1.Resources.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No definite, implementing method was found in the implementing class of the interface.
        /// </summary>
        internal static string AmbiguousMethodException {
            get {
                return ResourceManager.GetString("AmbiguousMethodException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Implementing class {0} could not be found.
        /// </summary>
        internal static string ClassNotFoundException {
            get {
                return ResourceManager.GetString("ClassNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unspecified Data Access Layer exception. Check the inner exception for details..
        /// </summary>
        internal static string DALException {
            get {
                return ResourceManager.GetString("DALException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unspecified infrastructure layer exception. Check the inner exception for details..
        /// </summary>
        internal static string InfrastructureException {
            get {
                return ResourceManager.GetString("InfrastructureException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Interface {0} could not be found.
        /// </summary>
        internal static string InterfaceNotFoundException {
            get {
                return ResourceManager.GetString("InterfaceNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Method {0} does not fulfill necessary criteria for aspect {1}.
        /// </summary>
        internal static string InvalidMethodForAspectException {
            get {
                return ResourceManager.GetString("InvalidMethodForAspectException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception occurred while preparing the invocation of a method on the RemotingServer. Check the inner exception for details..
        /// </summary>
        internal static string MessageProcessingException {
            get {
                return ResourceManager.GetString("MessageProcessingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No class that implements the IRemotingClient interface has been registered at the ObjectFactory for interface {0}.
        /// </summary>
        internal static string NoClientRegisteredException {
            get {
                return ResourceManager.GetString("NoClientRegisteredException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RemotedAspect called after call to method {0} on {1}.
        /// </summary>
        internal static string RemotedAspectAfterMethodCall {
            get {
                return ResourceManager.GetString("RemotedAspectAfterMethodCall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Registering RemotingClient {0} for type {1}.
        /// </summary>
        internal static string RemotedAspectInitialized {
            get {
                return ResourceManager.GetString("RemotedAspectInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception occurred while processing a remoted call on the client side. Check the inner exceptions for details.
        /// </summary>
        internal static string RemotingClientException {
            get {
                return ResourceManager.GetString("RemotingClientException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remoting invocation of method {0} for interface {1}.
        /// </summary>
        internal static string RemotingClientInvoke {
            get {
                return ResourceManager.GetString("RemotingClientInvoke", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sending invocation of method {0} for interface {1} as message with id {2} and {3} bytes of payload.
        /// </summary>
        internal static string RemotingClientInvokeSendingInvocation {
            get {
                return ResourceManager.GetString("RemotingClientInvokeSendingInvocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unspecified exception caught while processing a remoted invocation on the server side. Check the inner exception for details..
        /// </summary>
        internal static string RemotingServerException {
            get {
                return ResourceManager.GetString("RemotingServerException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoking method {0} on target {1}.
        /// </summary>
        internal static string RemotingServerInvoke {
            get {
                return ResourceManager.GetString("RemotingServerInvoke", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sending response of invocation for message with id {0}.
        /// </summary>
        internal static string RemotingServerProcessInvocationSendingResponse {
            get {
                return ResourceManager.GetString("RemotingServerProcessInvocationSendingResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Deserialized message {0} to invocation of method &quot;{1}&quot; in interface with GUID {2}.
        /// </summary>
        internal static string RemotingServerProcessMessageDeserialized {
            get {
                return ResourceManager.GetString("RemotingServerProcessMessageDeserialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invocation of message with id {0} returned value of type {1} and throw exception {2}.
        /// </summary>
        internal static string RemotingServerProcessMessageInvoked {
            get {
                return ResourceManager.GetString("RemotingServerProcessMessageInvoked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processing received message with id {0}.
        /// </summary>
        internal static string RemotingServerProcessMessageStart {
            get {
                return ResourceManager.GetString("RemotingServerProcessMessageStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Received message with id {0} and {1} bytes of payload.
        /// </summary>
        internal static string RemotingServerRunMsgReceived {
            get {
                return ResourceManager.GetString("RemotingServerRunMsgReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RemotingServer task started for instance of {0}.
        /// </summary>
        internal static string RemotingServerRunStarted {
            get {
                return ResourceManager.GetString("RemotingServerRunStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RemotingServer task is stopping.
        /// </summary>
        internal static string RemotingServerRunStopped {
            get {
                return ResourceManager.GetString("RemotingServerRunStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RemotingServer task will stop after waiting for {0} invocation-tasks....
        /// </summary>
        internal static string RemotingServerRunStopping {
            get {
                return ResourceManager.GetString("RemotingServerRunStopping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An exception occurred while processing the response of an invocation at the RemotingServer. Check the inner exception for details..
        /// </summary>
        internal static string ResponseProcessingException {
            get {
                return ResourceManager.GetString("ResponseProcessingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Return type or parameter type {0} of method {1} has not SerializableAttribute and therefore cannot be used on aspect {2}.
        /// </summary>
        internal static string TypeNotSerializableException {
            get {
                return ResourceManager.GetString("TypeNotSerializableException", resourceCulture);
            }
        }
    }
}
