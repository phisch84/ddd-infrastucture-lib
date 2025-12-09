using System;

namespace com.schoste.ddd.Infrastructure.V1.Logging.Void
{
    /// <summary>
    /// Implementation of the <see cref="Logging.ILog"/> interface and extension of the base class <see cref="Logging.Log"/>
    /// which ignores all log messages.
    /// </summary>
    public class Log : Logging.Log
    {
        public Log() : base()
        { }

        protected override void Initialize()
        {
        }

        protected override void DebugInternal(string? message, params object?[]? args)
        {
        }

        protected override void ErrorInternal(Exception? exception)
        {
        }

        protected override void ErrorInternal(Exception? exception, string? message, params object?[]? args)
        {
        }

        protected override void ErrorInternal(string? message, params object?[]? args)
        {
        }

        protected override void FatalInternal(Exception? exception)
        {
        }

        protected override void FatalInternal(Exception? exception, string? message, params object?[]? args)
        {
        }

        protected override void FatalInternal(string? message, params object?[]? args)
        {
        }

        protected override void InfoInternal(string? message, params object?[]? args)
        {
        }

        protected override void TraceInternal(string? message, params object?[]? args)
        {
        }

        protected override void TraceInternal(params object?[]? args)
        {
        }

        protected override void WarnInternal(string? message, params object?[]? args)
        {
        }
    }
}
