using System;
using System.Text;

namespace com.schoste.ddd.Infrastructure.V1.Logging
{
    using Infrastructure.V1.Resources;
    using Infrastructure.V1.Shared.Services;

    /// <summary>
    /// Abstract implementation of <see cref="ILog"/>.
    /// It defines all available <see cref="LogLevels"/> and defines the single-ton constructor
    /// for the whole logging logic.
    /// Implementing classes are supposed to derive from this class. Which implementation the application
    /// actually uses then needs to be define in the configuration for <see cref="ObjectFactory"/>.
    /// </summary>
    abstract public class Log : ILog
    {
        /// <summary>
        /// The logging levels the application supports
        /// </summary>
        [Flags]
        public enum LogLevels
        {
            /// <summary>
            /// Do not log at all
            /// </summary>
            None = 0,

            /// <summary>
            /// Log situations which will stop the application and there is nothing the user can do about it for system operators.
            /// </summary>
            Fatal = 1,

            /// <summary>
            /// Log situations where the user can and needs to provide input in order to prevent the stop of the application for system operators.
            /// </summary>
            Error = 2,

            /// <summary>
            /// Log unwanted situations for system operators.
            /// </summary>
            Warning = 4,

            /// <summary>
            /// Log additional information for system operators about a situation (not necessarily unwanted).
            /// </summary>
            Info = 8,

            /// <summary>
            /// Log information for developers
            /// </summary>
            Debug = 16,

            /// <summary>
            /// Log detailed information for developers which will help to reconstruct the process
            /// </summary>
            Trace = 32,
        }

        /// <inheritdoc/>
        virtual public LogLevels Level { get; set; }

        static protected ILog? LogInstance;

        /// <summary>
        /// Gets the instance to the logging logic
        /// </summary>
        static public ILog? Instance
        {
            get
            {
                if (LogInstance == null) LogInstance = ObjectFactory.GetInstance<ILog>();

                return LogInstance;
            }
        }

        /// <summary>
        /// Gets the full name of an object's instance type. If the instance is null, or the full name is null,
        /// <see cref="Resources.LogFormats.ClassInfoNULL"/> is returned.
        /// </summary>
        /// <param name="instance">The instance to get the full type name for</param>
        /// <returns>The full name of the instance's type, or a default value if it cannot be obtained</returns>
        static public string GetObjectTypeFullName(object? instance)
        {
            if (ReferenceEquals(null, instance)) return LogFormats.ClassInfoNULL;

            return GetObjectTypeFullName(instance.GetType());
        }

        /// <summary>
        /// Gets the full name of a <see cref="Type"/>. If the full name is null, <see cref="Resources.LogFormats.ClassInfoNULL"/> is returned.
        /// </summary>
        /// <param name="instance">The instance to get the full type name for</param>
        /// <returns>The full name of the instance's type, or a default value if it cannot be obtained</returns>
        static public string GetObjectTypeFullName(Type? type)
        {
            var typeFullName = ReferenceEquals(null, type) ? LogFormats.ClassInfoNULL : type.FullName;
                typeFullName = ReferenceEquals(null, typeFullName) ? LogFormats.ClassInfoNULL : typeFullName;

            return typeFullName;
        }

        /// <summary>
        /// Formats an exception to a log message according to the <paramref name="level"/>.
        /// If the <paramref name="level"/> has either the flag <see cref="LogLevels.Debug"/> or <see cref="LogLevels.Trace"/>
        /// then the stack trace will be included in the message.
        /// Otherwise it is only the exception type and the exception's message
        /// </summary>
        /// <param name="level">The log level to format the exception for</param>
        /// <param name="exception">The exception to print</param>
        /// <returns>Printable text of the exception</returns>
        static public string GetLogMessage(LogLevels level, Exception exception)
        {
            try
            {
                var _exception = exception;
                var msgBuilder = new StringBuilder();

                do
                {
                    var exceptionClass = GetObjectTypeFullName(_exception);
                    var exceptionMessage = (_exception == null) ? String.Empty : _exception.Message;
                    var exceptionStackTrace = (_exception == null) ? String.Empty : _exception.StackTrace;
                        exceptionStackTrace = (exceptionStackTrace == null) ? String.Empty : exceptionStackTrace;
                    var logMsg = (level.HasFlag(LogLevels.Debug) || level.HasFlag(LogLevels.Trace))
                                 ? String.Format(LogFormats.GetMessageExceptionDebug, exceptionClass, exceptionMessage, exceptionStackTrace)
                                 : String.Format(LogFormats.GetMessageException, exceptionClass, exceptionMessage);

                    msgBuilder.AppendLine(logMsg);

                    _exception = _exception?.InnerException;
                }
                while (_exception != null);

                return msgBuilder.ToString();
            }
            catch (Exception ex)
            {
                var exceptionClass = typeof(Log).FullName;
                    exceptionClass = (exceptionClass == null) ? LogFormats.ClassInfoNULL : exceptionClass;

                return String.Format(LogFormats.GetMessageException, typeof(Log).FullName, ex.Message);
            }
        }

        protected Log()
        {
            try
            {
                this.Initialize();
            }
            catch (Exceptions.InfrastructureException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new Exceptions.InfrastructureException(ex);
            }
        }

        /// <summary>
        /// Called by the constructor. Shall be overwritten by the implementing class
        /// to initialize the instance of the logger.
        /// </summary>
        abstract protected void Initialize();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void FatalInternal(Exception? exception);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void FatalInternal(Exception? exception, string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void FatalInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void ErrorInternal(Exception? exception);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void ErrorInternal(Exception? exception, string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void ErrorInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void WarnInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void InfoInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void DebugInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void TraceInternal(string? message, params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        abstract protected void TraceInternal(params object?[]? args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Fatal(Exception exception)
        {
            if (this.Level.HasFlag(LogLevels.Fatal)) this.FatalInternal(exception);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Fatal(Exception exception, string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Fatal)) this.FatalInternal(exception, message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Fatal(string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Fatal)) this.FatalInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Error(Exception exception)
        {
            if (this.Level.HasFlag(LogLevels.Error)) this.ErrorInternal(exception);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Error(Exception exception, string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Error)) this.ErrorInternal(exception, message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Error(string message, params object[] args)
        {
           if (this.Level.HasFlag(LogLevels.Error)) this.ErrorInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Warn(string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Warning)) this.WarnInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Info(string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Info)) this.InfoInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Debug(string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Debug)) this.DebugInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Trace(string message, params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Trace)) this.TraceInternal(message, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        virtual public void Trace(params object[] args)
        {
            if (this.Level.HasFlag(LogLevels.Trace)) this.TraceInternal(args);
        }
    }

}
