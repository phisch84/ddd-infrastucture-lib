namespace com.schoste.ddd.Infrastructure.V1.Logging
{
    using com.schoste.ddd.Infrastructure.V1.Aspects;
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
        /// Tests of logging an exception does not cause an error
        /// </summary>
        [TestMethod]
        public void TestLogException()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");

            Log.Instance.Fatal(new System.Exception());
        }

        /// <summary>
        /// Tests of logging an a trace with no message
        /// </summary>
        [TestMethod]
        public void TestLogTraceNoMessage()
        {
            Assert.IsNotNull(Log.Instance, "No logger has been initialized.");

            Log.Instance.Trace();
        }
    }
}
