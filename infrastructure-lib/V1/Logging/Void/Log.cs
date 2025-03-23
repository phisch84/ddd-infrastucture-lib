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

        public override void Fatal(Exception exception)
        {
        }

        public override void Fatal(Exception exception, string message, params object[] args)
        {
        }

        public override void Fatal(string message, params object[] args)
        {
        }

        public override void Error(Exception exception)
        {
        }

        public override void Error(Exception exception, string message, params object[] args)
        {
        }

        public override void Error(string message, params object[] args)
        {
        }

        public override void Warn(string message, params object[] args)
        {
        }

        public override void Info(string message, params object[] args)
        {
        }

        public override void Debug(string message, params object[] args)
        {
        }

        public override void Trace(string message, params object[] args)
        {
        }

        public override void Trace(params object[] args)
        {
        }
    }
}
