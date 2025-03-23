using System;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using V1.Remoting;
    using V1.Shared.Services;

    /// <summary>
    /// Test suite to ensure correct implementation of <see cref="RemotedAspect"/>
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class RemotedAspectUnitTests
    {
        static private TestContext? testContextInstance;
        static private IRemotingServer? remotingServerInstance;

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
        /// An interface to a simple test class
        /// </summary>
        public interface ITestClass
        {
            public bool MethodWasCalled {  get; set; }

            void TestMethodVoid();

            string? TestMethodString(int param);
        }

        /// <summary>
        /// The implementation of the local class behind <see cref="ITestClass"/> which's methods have the
        /// <see cref="RemotedAspect"/> method attribute.
        /// </summary>
        public class TestClass : ITestClass
        {
            public bool MethodWasCalled { get; set; }

            public TestClass()
            {
                this.MethodWasCalled = false;
            }

            [RemotedAspect]
            public string? TestMethodString(int param)
            {
                return default;
            }

            [RemotedAspect]
            public void TestMethodVoid()
            {
            }
        }

        /// <summary>
        /// The implementation of the remote class behind <see cref="ITestClass"/> which's methods are supposed
        /// to be executed by <see cref="RemotingServer"/>.
        /// </summary>
        public class RemoteTestClass : ITestClass
        {
            public bool MethodWasCalled { get; set; }

            public RemoteTestClass()
            {
                this.MethodWasCalled = false;
            }

            public string? TestMethodString(int param)
            {
                this.MethodWasCalled = true;

                return Convert.ToString(param);
            }

            public void TestMethodVoid()
            {
                this.MethodWasCalled = true;
            }
        }

        /// <summary>
        /// Sets up the <see cref="ObjectFactory"/> so classes are decorated properly by the <see cref="AspectProxy{T}"/>.
        /// </summary>
        /// <param name="testContext"></param>
        [ClassInitialize]
        static public void Initialize(TestContext testContext)
        {
            testContextInstance = testContext;

            var ifName = typeof(ITestClass).FullName;
            var clsName = typeof(TestClass).FullName;

            ObjectFactory.Register(typeof(ITestClass), typeof(TestClass));
            ObjectFactory.Register(typeof(IRemotingClient), typeof(Remoting.Stream.AnonPipes.JsonRemotingClient));
            ObjectFactory.Register(typeof(IRemotingServer), typeof(Remoting.Stream.AnonPipes.RemotingServer));
            ObjectFactory.Register(typeof(ISerializer), typeof(Remoting.Mocked.Serializer));

            var serializer = ObjectFactory.CreateInstance<ISerializer>();
            var remotingClient = ObjectFactory.CreateInstance<IRemotingClient>(serializer);
            var remotingServer = ObjectFactory.CreateInstance<IRemotingServer>(serializer, ((string[])remotingClient.Configuration)[0], ((string[])remotingClient.Configuration)[1]);

            ObjectFactory.RegisterSingleton<ISerializer>(serializer);
            ObjectFactory.RegisterSingleton<IRemotingClient>(remotingClient);
            ObjectFactory.RegisterSingleton<IRemotingServer>(remotingServer);

            remotingServerInstance = ObjectFactory.GetInstance<IRemotingServer>();
            remotingServerInstance?.Start();
        }

        [ClassCleanup]
        static public void Cleanup()
        {
            remotingServerInstance?.Stop();
        }

        /// <summary>
        /// Tests if a simple call with no parameters that returns void works as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void TestMethodVoid()
        {
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();

            localInstance.TestMethodVoid();
            
            Assert.IsTrue(remoteInstance?.MethodWasCalled);
        }

        /// <summary>
        /// Tests if a simple class with an integer parameter that returns a string works as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void TestMethodString()
        {
            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var actual = localInstance.TestMethodString(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.AreEqual(Convert.ToString(expected), actual);
        }

        /// <summary>
        /// Tests if two subsequent calls work as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public void TestMethodsVoidAndString()
        {
            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();

                         localInstance.TestMethodVoid();
            var actual = localInstance.TestMethodString(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.AreEqual(Convert.ToString(expected), actual);
        }

    }
}
