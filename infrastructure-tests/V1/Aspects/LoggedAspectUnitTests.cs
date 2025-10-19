using System;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using com.schoste.ddd.Infrastructure.V1.Logging;
    using V1.Shared.Services;

    /// <summary>
    /// Test suite for <see cref="LoggedAspect"/>
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class LoggedAspectUnitTests
    {
        static private TestContext? testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext? TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        /// <summary>
        /// Interface to the test class which needs to be registered with <see cref="ObjectFactory.Register(Type, Type)"/>.
        /// </summary>
        public interface ITestClass
        {
            string TestLoggingWithException(int param1, bool param2);
            string TestLoggingNoException();
        }

        /// <summary>
        /// Implementation of <see cref="ITestClass"/> which needs to be registered with <see cref="ObjectFactory.Register(Type, Type)"/>
        /// so it can be created using <see cref="ObjectFactory.CreateInstance{T}(object[])"/>.
        /// </summary>
        public class TestClass : ITestClass
        {
            [LoggedAspect]
            public string TestLoggingWithException(int param1, bool param2)
            {
                throw new Exception();
            }

            [LoggedAspect]
            public string TestLoggingNoException()
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes the tests:
        /// 1.) Registers the logger as singleton
        /// 2.) Registers the test class
        /// </summary>
        /// <param name="testContext"></param>
        [ClassInitialize]
        static public void Initialize(TestContext testContext)
        {
            testContextInstance = testContext;

            // 1. Register the logger as singleton
            ObjectFactory.Register(typeof(ILog), typeof(Logging.Mocked.Log));
            ObjectFactory.RegisterSingleton<ILog>(ObjectFactory.CreateInstance<ILog>());

            // 2. Register the test class
            ObjectFactory.Register(typeof(ITestClass), typeof(TestClass));
        }

        /// <summary>
        /// Asserts that <see cref="LoggedAspect.AfterMethodCall(DateTime, TimeSpan, object?, object?[]?, ref object?, ref Exception?, Type?, System.Reflection.MethodInfo?)"/>
        /// is called after the annotated methods of the test class were invoked.
        /// </summary>
        [TestMethod]
        public void TestAfterMethodCall()
        {

            var obj = ObjectFactory.CreateInstance<ITestClass>();
            var oldLogEntries = Logging.Mocked.Log.Entries;

            try
            {
                var actualInf = obj.TestLoggingNoException();
                var actualDbg = obj.TestLoggingWithException(1, false);
            }
            catch (Exception)
            { }

            var newLogEntries = Logging.Mocked.Log.Entries;

            Assert.IsTrue(newLogEntries.Count > oldLogEntries.Count);
        }
    }
}
