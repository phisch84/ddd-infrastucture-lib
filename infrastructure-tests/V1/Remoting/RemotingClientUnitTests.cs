using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Remoting
{
    using Infrastructure.V1.Shared.Services;

    /// <summary>
    /// Tests the correct logic of <see cref="RemotingClient"/> (implemented by <see cref="Remoting.Mocked.LocalClient"/>)
    /// and <see cref="RemotingServer"/> (implemented by <see cref="Remoting.Mocked.LocalServer"/>)
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class RemotingClientUnitTests
    {
        static private TestContext testContextInstance;

        public interface ITestClass
        {
            public bool MethodWasCalled { get; set; }

            public Task TestMethodVoid();

            public Task<string?> TestMethodString(int param);
        }

        public class TestClass : ITestClass
        {
            public bool MethodWasCalled { get; set; }

            public TestClass()
            {
                this.MethodWasCalled = false;
            }

            public async Task<string?> TestMethodString(int param)
            {
                this.MethodWasCalled = true;

                return param.ToString();
            }

            public async Task TestMethodVoid()
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
        [Timeout(1000)]
        public void TestInvokeVoid()
        {
            var testClassInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfo = testClassInstance.GetType().GetMethod(nameof(TestClass.TestMethodVoid));
            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var client = new Mocked.LocalClient(serializer);

            Assert.IsNotNull(testClassMethodInfo);

            client.Invoke(new object[0], out var returnValue, out var exception, typeof(ITestClass), testClassMethodInfo);

            Assert.IsTrue(testClassInstance.MethodWasCalled);
            Assert.IsNull(exception);
            Assert.IsNull(returnValue);
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestInvokeString()
        {
            var testClassInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfo = testClassInstance.GetType().GetMethod(nameof(TestClass.TestMethodString));
            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var client = new Mocked.LocalClient(serializer);
            var param = 2;

            Assert.IsNotNull(testClassMethodInfo);

            client.Invoke(new object[] { param }, out var returnValue, out var exception, typeof(ITestClass), testClassMethodInfo);

            Assert.IsTrue(testClassInstance.MethodWasCalled);
            Assert.IsNull(exception);
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(param.ToString(), returnValue.ToString());
        }

        [TestMethod]
        [Timeout(1000)]
        public void TestInvokeVoidAndString()
        {
            var testClassInstance = ObjectFactory.GetInstance<ITestClass>();
            var testClassMethodInfoVoid = testClassInstance.GetType().GetMethod(nameof(TestClass.TestMethodVoid));
            var testClassMethodInfoString = testClassInstance.GetType().GetMethod(nameof(TestClass.TestMethodString));
            var serializer = ObjectFactory.GetInstance<ISerializer>();
            var client = new Mocked.LocalClient(serializer);
            var param = 2;

            Assert.IsNotNull(testClassMethodInfoVoid);
            Assert.IsNotNull(testClassMethodInfoString);

            client.Invoke(new object[0], out var returnValueVoid, out var exceptionVoid, typeof(ITestClass), testClassMethodInfoVoid);
            client.Invoke(new object[] { param }, out var returnValueString, out var exceptionString, typeof(ITestClass), testClassMethodInfoString);

            Assert.IsTrue(testClassInstance.MethodWasCalled);

            Assert.IsNull(exceptionVoid);
            Assert.IsNull(returnValueVoid);

            Assert.IsNull(exceptionString);
            Assert.IsNotNull(returnValueString);
            Assert.AreEqual(param.ToString(), returnValueString.ToString());
        }
    }
}
