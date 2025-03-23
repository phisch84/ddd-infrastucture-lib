using System;
using static com.schoste.ddd.Infrastructure.V1.Logging.Log;

namespace com.schoste.ddd.Infrastructure.V1.Logging
{
    /// <summary>
    /// Interface to the logging abstraction of the application
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Gets or sets which log messages should be written
        /// </summary>
        LogLevels Level { get; set; }

        /// <summary>
        /// Logs a fatal event. A fatal event is a situation where the application is going to terminate.
        /// (Therefore it should be called only from the application layer)
        /// </summary>
        /// <param name="exception">The exception that lead to the fatal event</param>
        void Fatal(Exception exception);

        /// <summary>
        /// Logs a fatal event. A fatal event is a situation where the application is going to terminate.
        /// (Therefore it should be called only from the application layer)
        /// </summary>
        /// <param name="exception">The exception that lead to the fatal event</param>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Fatal(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs a fatal event. A fatal event is a situation where the application is going to terminate.
        /// (Therefore it should be called only from the application layer)
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        /// Logs an error. An error is a situation where the application might terminate
        /// (if the error isn't handled by the calling method or an upper layer).
        /// </summary>
        /// <param name="exception">The exception that lead to the error</param>
        void Error(Exception exception);

        /// <summary>
        /// Logs an error. An error is a situation where the application might terminate
        /// (if the error isn't handled by the calling method or an upper layer).
        /// </summary>
        /// <param name="exception">The exception that lead to the error</param>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Error(Exception exception, string message, params object[] args);

        /// <summary>
        /// Logs an error. An error is a situation where the application might terminate
        /// (if the error isn't handled by the calling method or an upper layer).
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// Logs a warning. A warning is a solved problem that might lead to an error
        /// under different circumstances or in the future.
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Logs an information. An information simply tells the user about what is going on
        /// in the application. They must no contain sensitive/secret information.
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Logs debug information. Debug information are more detailed than just information.
        /// They should contain information that help to trace a specific work flow and may also
        /// contain sensitive/secret information.
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Logs debug information in a higher detail.
        /// They should contain information that help to trace a specific work flow and may also
        /// contain sensitive/secret information.
        /// </summary>
        /// <param name="message">A message template to write to the logs</param>
        /// <param name="args">Arguments to fill the template with</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// Logs method calls to help tracing a work flow
        /// </summary>
        /// <param name="args">Parameters that were passed to the called method</param>
        void Trace(params object[] args);
    }
}
