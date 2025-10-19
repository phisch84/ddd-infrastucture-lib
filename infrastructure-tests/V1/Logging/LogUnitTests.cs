namespace com.schoste.ddd.Infrastructure.V1.Logging
{
    using V1.Shared.Services;

    /// <summary>
    /// Unit tests of <see cref="Log"/>.
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class LogUnitTests
    {
        /// <summary>
        /// Initializes static properties of the class
        /// </summary>
        [ClassInitialize]
        static public void SetUp(TestContext testContext)
        {
            ObjectFactory.Register(typeof(ILog), typeof(Mocked.Log));
            ObjectFactory.RegisterSingleton<ILog>(ObjectFactory.CreateInstance<ILog>());
        }

        /// <summary>
        /// Tests if <see cref="Log.Level"/> was initialized with all possible levels
        /// </summary>
        [TestMethod]
        public void TestCorrectInitialization()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Trace));
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Debug));
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Info));
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Warning));
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Error));
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Fatal));
        }

        /// <summary>
        /// Tests of logging an exception on fatal level.
        /// Tests the method <see cref="Log.Fatal(System.Exception)"/>.
        /// </summary>
        [TestMethod]
        public void TestLogFatalWithException()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Fatal), "Log levels do not include Fatal");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Fatal(new System.Exception());

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a fatal event with an exception and a message.
        /// Tests the method <see cref="Log.Fatal(System.Exception, string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestLogFatalWithExceptionAndMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Fatal), "Log levels do not include Fatal");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Fatal(new System.Exception(), "Test {0}", "TestLogFatalWithExceptionAndMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a fatal event with no exception but a message.
        /// Tests the method <see cref="Log.Fatal(string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestLogFatalWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Fatal), "Log levels do not include Fatal");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Error("Test {0}", "TestLogFatalWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging an exception on error level.
        /// Tests the method <see cref="Log.Error(System.Exception)"/>.
        /// </summary>
        [TestMethod]
        public void TestLogErrorWithException()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Error), "Log levels do not include Error");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Error(new System.Exception());

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a error event with an exception and a message.
        /// Tests the method <see cref="Log.Error(System.Exception, string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestLogErrorWithExceptionAndMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Error), "Log levels do not include Error");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Error(new System.Exception(), "Test {0}", "TestLogErrorWithExceptionAndMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a fatal event with no exception but a message.
        /// Tests the method <see cref="Log.Error(string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestLogErrorWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Error), "Log levels do not include Error");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Error("Test {0}", "TestLogErrorWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a warning with a message.
        /// Tests the method <see cref="Log.Warn(string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestWarnWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Warning), "Log levels do not include Warning");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Warn("Test {0}", "TestWarnWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging an info with a message.
        /// Tests the method <see cref="Log.Info(string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestInfoWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Info), "Log levels do not include Info");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Info("Test {0}", "TestInfoWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging a debug info with a message.
        /// Tests the method <see cref="Log.Debug(string, object[])"/>.
        /// </summary>
        [TestMethod]
        public void TestDebugWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Debug), "Log levels do not include Debug");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Debug("Test {0}", "TestDebugWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging an a trace with no message.
        /// Tests the method <see cref="Log.Trace(params object[] args)"/>.
        /// </summary>
        [TestMethod]
        public void TestLogTraceNoMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Trace), "Log levels do not include Trace");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Trace();

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }

        /// <summary>
        /// Tests of logging an a trace with no message.
        /// Tests the method <see cref="Log.Trace(params object[] args)"/>.
        /// </summary>
        [TestMethod]
        public void TestLogTraceWithMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");
            Assert.IsTrue(Log.Instance.Level.HasFlag(Log.LogLevels.Trace), "Log levels do not include Trace");

            var oldLogEntries = Logging.Mocked.Log.Entries;

            Log.Instance.Trace("Test {0}", "TestLogTraceWithMessage");

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }
    }
}
