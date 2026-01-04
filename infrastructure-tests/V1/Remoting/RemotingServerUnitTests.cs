namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    using Remoting.Mocked;
    using Shared.Services;
    using System;
    using System.Threading.Tasks;

    [TestClass]
    [TestCategory("Unit Test")]
    public class RemotingServerUnitTests
    {
        static private TestContext testContextInstance;

        public interface ITestClass
        {
            public bool MethodWasCalled { get; set; }

            void TestMethodVoid();

            string? TestMethodString(int param);

            Task<string?> TestMethodStringAsync(int param);

            Task TestMethodVoidAsync();
        }

        public class TestClass : ITestClass
        {
            public bool MethodWasCalled { get; set; }

            public TestClass()
            {
                this.MethodWasCalled = false;
            }

            public string? TestMethodString(int param)
            {
                this.MethodWasCalled = true;

                return param.ToString();
            }

            public void TestMethodVoid()
            {
                this.MethodWasCalled = true;
            }

            public async Task<string?> TestMethodStringAsync(int param)
            {
                this.MethodWasCalled = true;
                await Task.Delay(1);
                return param.ToString();
            }

            public async Task TestMethodVoidAsync()
            {
                this.MethodWasCalled = true;
            }

        }

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [ClassInitialize]
        static public void Initialize(TestContext testContext)
        {
            testContextInstance = testContext;

            var ifName = typeof(ITestClass).FullName;
            var clsName = typeof(TestClass).FullName;

            ObjectFactory.Configuration.InterfaceToImplementationMap[ifName] = clsName; // Configure test class
            ObjectFactory.RegisterSingleton<ITestClass>(ObjectFactory.CreateInstance<ITestClass>());

            ObjectFactory.Configuration.InterfaceToImplementationMap[typeof(ISerializer).FullName] = typeof(Mocked.Serializer).FullName; // Configure the serializer to use
            ObjectFactory.RegisterSingleton(ObjectFactory.CreateInstance<ISerializer>());
        }

        [TestInitialize]
        public void Reset()
        {
            ObjectFactory.GetInstance<ITestClass>().MethodWasCalled = false;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TestProcessMessageVoid()
        {
            var tcInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfo = tcInstance.GetType().GetMethod(nameof(TestClass.TestMethodVoid));

            Assert.IsNotNull(testClassMethodInfo);

            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var cliInstance = new LocalClient(serializer);
            var srvInstance = cliInstance.LocalServer;
            var methodInvocationMsg = cliInstance.SerializeInvocation([], typeof(ITestClass), testClassMethodInfo);
            var actualDTO = srvInstance.ProcessMessage(0, methodInvocationMsg);

            Assert.IsTrue(tcInstance.MethodWasCalled);
        }

        [TestMethod]
        public void TestProcessMessageStringAsync()
        {
            var tcInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfo = tcInstance.GetType().GetMethod(nameof(TestClass.TestMethodStringAsync));

            Assert.IsNotNull(testClassMethodInfo);
            
            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var cliInstance = new LocalClient(serializer);
            var srvInstance = cliInstance.LocalServer;
            var methodInvocationMsg = cliInstance.SerializeInvocation(new object[] { 3 } , typeof(ITestClass), testClassMethodInfo);
            var actualDTO = srvInstance.ProcessMessage(0, methodInvocationMsg);

            Assert.IsTrue(tcInstance.MethodWasCalled);
        }

        [TestMethod]
        public void TestProcessMessageVoidAsync()
        {
            var tcInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfo = tcInstance.GetType().GetMethod(nameof(TestClass.TestMethodVoidAsync));

            Assert.IsNotNull(testClassMethodInfo);

            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var cliInstance = new LocalClient(serializer);
            var srvInstance = cliInstance.LocalServer;
            var methodInvocationMsg = cliInstance.SerializeInvocation([], typeof(ITestClass), testClassMethodInfo);
            var actualDTO = srvInstance.ProcessMessage(0, methodInvocationMsg);

            Assert.IsTrue(tcInstance.MethodWasCalled);
        }

    }
}
