using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Aspects
{
    using V1.Logging;
    using V1.Remoting;
    using V1.Remoting.Exceptions;
    using V1.Exceptions;
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

        #region Test Classes
        /// <summary>
        /// An interface to a simple test class
        /// </summary>
        public interface ITestClass
        {
            public bool MethodWasCalled {  get; set; }

            void TestMethodVoid();

            Task<IEnumerable<int>?> TestMethodIEnumInt(int param);

            Task TestMethodVoidAsync();

            Task<string?> TestMethodStringAsync(int param);

            Task TestMethodVoidWithExceptionAsync();

            Task<string?> TestMethodStringWithExceptionAsync(int param);
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
            public Task<IEnumerable<int>?> TestMethodIEnumInt(int param)
            {
                return default;
            }

            [RemotedAspect]
            public void TestMethodVoid()
            {
            }

            [RemotedAspect]
            public async Task<string?> TestMethodStringAsync(int param)
            {
                return default;
            }

            [RemotedAspect]
            public async Task TestMethodVoidAsync()
            {
            }

            [RemotedAspect]
            public async Task TestMethodVoidWithExceptionAsync()
            {
            }

            [RemotedAspect]
            public async Task<string?> TestMethodStringWithExceptionAsync(int param)
            {
                return default;
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

            public async Task<IEnumerable<int>?> TestMethodIEnumInt(int param)
            {
                this.MethodWasCalled = true;

                return new int[] { param, param };
            }

            public void TestMethodVoid()
            {
                this.MethodWasCalled = true;
            }

            public async Task<string?> TestMethodStringAsync(int param)
            {
                this.MethodWasCalled = true;

                return Convert.ToString(param);
            }

            public async Task TestMethodVoidAsync()
            {
                this.MethodWasCalled = true;
            }

            public async Task TestMethodVoidWithExceptionAsync()
            {
                this.MethodWasCalled = true;

                throw new KeyNotFoundException();
            }

            public async Task<string?> TestMethodStringWithExceptionAsync(int param)
            {
                this.MethodWasCalled = true;

                throw new KeyNotFoundException();
            }
        }
        #endregion

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

            ObjectFactory.Register(typeof(ILog), typeof(Logging.Mocked.Log));
            ObjectFactory.RegisterSingleton<ILog>(ObjectFactory.CreateInstance<ILog>());

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
        static public void CleanUpAfterAllTests()
        {
            remotingServerInstance?.Stop();
        }

        [TestCleanup]
        public void CleanUpAfterEachTest()
        {
            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();

            if (remoteInstance != null) remoteInstance.MethodWasCalled = false;
        }

        /// <summary>
        /// Tests if an intended, simple call with no parameters that returns void (but isn't async)
        /// will trigger a validation exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InfrastructureException))]
        [Timeout(1000)]
        public async Task TestMethodVoid()
        {
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            
            localInstance.TestMethodVoid();

            Assert.IsFalse(remoteInstance?.MethodWasCalled);
        }

        /// <summary>
        /// Tests if a simple call with no parameters that returns void works as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public async Task TestMethodVoidAsync()
        {
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var voidTask = localInstance.TestMethodVoidAsync();
            
            await voidTask;

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
        }

        /// <summary>
        /// Tests if a simple class with an integer parameter that returns a string works as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public async Task TestMethodStringAsync()
        {
            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var actual = await localInstance.TestMethodStringAsync(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.AreEqual(Convert.ToString(expected), actual);
        }

        /// <summary>
        /// Tests if two subsequent calls work as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public async Task TestMethodsVoidAndStringAsync()
        {
            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();

                         await localInstance.TestMethodVoidAsync();
            var actual = await localInstance.TestMethodStringAsync(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.AreEqual(Convert.ToString(expected), actual);
        }

        /// <summary>
        /// Tests if a simple class with an integer parameter that returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>
        /// throws an <see cref="RemotingServerException"/> if <see cref="System.Int32[]"/> wasn't registered at the serializer
        /// using <see cref="ISerializer.RegisterGuidForType(Guid, Type)"/>.
        /// </summary>
        /// <remarks>
        /// If this test runs until its timeout, then the <see cref="RemotingServer"/> failed to send a response for whatever reason.
        /// </remarks>
        [TestMethod]
        [Timeout(10000)]
        [ExpectedException(typeof(RemotingServerException))]
        public async Task TestMethodIEnumerableIntAsyncUnregistered()
        {
            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var actual = await localInstance.TestMethodIEnumInt(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
        }

        /// <summary>
        /// Tests if a simple class with an integer parameter that returns an <see cref="IEnumerable{T}"/> of <see cref="int"/>
        /// works as expected
        /// </summary>
        [TestMethod]
        [Timeout(1000)]
        public async Task TestMethodIEnumerableIntAsyncRegistered()
        {
            var serializer = ObjectFactory.GetInstance<ISerializer>();

            Assert.IsNotNull(serializer);

            // Register the int[] type at the serializer instance.
            // This registration makes the difference to TestMethodIEnumerableIntAsyncUnregistered().
            serializer.RegisterGuidForType(Guid.NewGuid(), typeof(int[]));

            var expected = 5;
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var actual = await localInstance.TestMethodIEnumInt(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.IsNotNull(actual);
            CollectionAssert.AreEqual(new int[] { expected, expected }, new List<int>(actual!));

            serializer.RegisterGuidForType(Guid.Empty, typeof(int[]));
        }

        /// <summary>
        /// Tests if a simple call with no parameters that will throw an exception works as expected
        /// </summary>
        [Ignore("The expected message is only thrown if putting a break point at the end of RemoteTestClass.TestMethodVoidWithExceptionAsync()")]
        [TestMethod]
        [Timeout(1000)]
        [ExpectedException(typeof(RemoteMethodException))]
        public async Task TestMethodVoidWithExceptionAsync()
        {
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var voidTask = localInstance.TestMethodVoidWithExceptionAsync();

            await voidTask;

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
        }

        [TestMethod]
        [Timeout(1000)]
        [ExpectedException(typeof(RemoteMethodException))]
        public async Task TestMethodStringWithExceptionAsync()
        {
            var localInstance = ObjectFactory.CreateInstance<ITestClass>();

            ObjectFactory.RegisterSingleton<ITestClass>(new RemoteTestClass());

            var remoteInstance = ObjectFactory.GetInstance<ITestClass>();
            var expected = 5;
            var actual = await localInstance.TestMethodStringWithExceptionAsync(expected);

            Assert.IsTrue(remoteInstance?.MethodWasCalled);
            Assert.AreEqual(Convert.ToString(expected), actual);
        }
    }
}
