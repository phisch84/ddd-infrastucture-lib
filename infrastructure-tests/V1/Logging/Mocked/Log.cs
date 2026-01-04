using System;
using System.Collections.Generic;
using System.Text;

namespace com.schoste.ddd.Infrastructure.V1.Logging.Mocked
{
    using Shared.Services;
    using System.Linq;

    /// <summary>
    /// Mock override of <see cref="Logging.Log"/> that does not actually write log entries
    /// but stores them in a list so they can be checked afterwards
    /// </summary>
    public class Log : Logging.Log
    {
        /// <summary>
        /// Data model of a log entry
        /// </summary>
        public class Entry
        {
            public DateTime DateTime { get; set; }
            public Log.LogLevels Level { get; set; }
            public string? Message { get; set; }
            public Exception? Exception { get; set; }
        }

        static private IList<Entry> logEntries = new List<Entry>();

        /// <summary>
        /// Gets all entries that were logged
        /// </summary>
        static public IList<Entry> Entries
        {
            get
            {
                return new List<Entry>(logEntries);
            }
        }

        static private string[] getParameters(System.Reflection.MethodBase method)
        {
            var parameterInfos = method.GetParameters();
            var parameters = new string[parameterInfos.Length];

            for (var i = 0; i < parameterInfos.Length; i++) parameters[i] = parameterInfos[i].ParameterType.FullName;

            return parameters;
        }

        static private string[] getCallers(System.Diagnostics.StackTrace stackTrace)
        {
            var callerFrameIndex = 2;

            if (stackTrace.FrameCount < callerFrameIndex) callerFrameIndex = 0;

            var callerFrames = new string[stackTrace.FrameCount - callerFrameIndex];

            for (var i = callerFrameIndex; i < stackTrace.FrameCount; i++)
            {
                var callerFrame = stackTrace.GetFrame(i);
                var caller = callerFrame.GetMethod();

                // skip frames of the Aspect Proxy and the ILog interface to get to the real calling method.
                if (caller.ReflectedType.GUID.Equals(typeof(AspectProxy<object>).GUID))
                {
                    i = i + 2; callerFrameIndex = (i + 1);

                    if (stackTrace.FrameCount < callerFrameIndex) continue;
                    else callerFrames = new string[stackTrace.FrameCount - callerFrameIndex];

                    continue;
                }

                var callerClass = caller.ReflectedType.FullName;
                var callerParams = getParameters(caller);
                var callerMethod = caller.Name;
                var fullCallerName = String.Format(LogFormats.GetCallers, callerClass, callerMethod, String.Join(", ", callerParams));

                callerFrames[i - callerFrameIndex] = fullCallerName;
            }

            return callerFrames;
        }

        static protected string GetLogMessage(LogLevels level, string message, params object[] args)
        {
            try
            {
                return String.Format(message, args);

                //if (!level.HasFlag(LogLevels.Trace))
                //{
                //    return String.Format(message, args);
                //}
                //else
                //{
                //    var callers = getCallers(new System.Diagnostics.StackTrace());
                //    var msg = String.Format(message, args);

                //    return String.Format(LogFormats.GetMessageTrace, String.Join(Environment.NewLine, callers), msg);
                //}
            }
            catch
            {
                return String.Format("{0} [{0}]", message, String.Join(", ", args));
            }
        }

        static protected string GetLogMessage(LogLevels level, Exception exception)
        {
            try
            {
                var _exception = exception;
                var msgBuilder = new StringBuilder();

                while (_exception != null)
                {
                    var exceptionClass = (exception == null) ? "NULL" : exception.GetType().FullName;
                    var exceptionMessage = (exception == null) ? "" : exception.Message;
                    var exceptionStackTrace = (exception == null) ? "" : exception.StackTrace;
                    var logMsg = String.Format(LogFormats.GetMessageExceptionDebug, exceptionClass, exceptionMessage, exceptionStackTrace);

                    msgBuilder.AppendLine(logMsg);

                    _exception = exception.InnerException;
                }

                return msgBuilder.ToString();
            }
            catch (Exception ex)
            {
                return String.Format(LogFormats.GetMessageException, typeof(Log).FullName, ex.Message);
            }
        }

        protected override void Initialize()
        {
            this.Level = LogLevels.Trace | LogLevels.Debug | LogLevels.Info | LogLevels.Warning | LogLevels.Error | LogLevels.Fatal;
        }

        public Log() : base() { }

        ///<inheritdoc/>
        protected override void DebugInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Debug,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void ErrorInternal(Exception? exception)
        {
            var msg = GetLogMessage(Level, exception);

            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Error,
                    Message = msg,
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void ErrorInternal(Exception? exception, string? message, params object?[]? args)
        {
            var msg = GetLogMessage(Level, exception);

            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Error,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void ErrorInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Error,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void FatalInternal(Exception? exception)
        {
            var msg = GetLogMessage(Level, exception);

            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Fatal,
                    Message = msg,
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void FatalInternal(Exception? exception, string? message, params object?[]? args)
        {
            var msg = GetLogMessage(Level, exception);

            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Fatal,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void FatalInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Fatal,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void InfoInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Info,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void TraceInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Trace,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToShortTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void WarnInternal(string? message, params object?[]? args)
        {
            lock (logEntries)
            {
                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Warning,
                    Message = String.Format(message, args),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }

        ///<inheritdoc/>
        protected override void TraceInternal(params object?[]? args)
        {
            lock (logEntries)
            {
                var argList = new List<object>(args);
                var objsToStr = argList.Select(obj => obj.ToString());

                var entry = new Entry()
                {
                    DateTime = DateTime.Now,
                    Level = Log.LogLevels.Trace,
                    Message = String.Format(LogFormats.GetMessageTrace, String.Join(", ", objsToStr)),
                    Exception = null,
                };

                Console.Out.WriteLine("{0}: {1}", entry.DateTime.ToLongTimeString(), entry.Message);

                logEntries.Add(entry);
            }
        }
    }
}
