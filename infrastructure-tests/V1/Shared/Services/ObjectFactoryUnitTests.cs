namespace com.schoste.ddd.Infrastructure.V1.Shared.Services
{
    /// <summary>
    /// Unit tests of the <see cref="ObjectFactory"/>
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class ObjectFactoryUnitTests
    {
        /// <summary>
        /// Clears all configurations of the <see cref="ObjectFactory"/> before each test run
        /// </summary>
        [TestInitialize]
        public void ResetObjectFactoryConfiguration()
        {
            ObjectFactory.Configuration.InterfaceToInstanceMap.Clear();
            ObjectFactory.Configuration.InterfaceToImplementationMap.Clear();
        }

        /// <summary>
        /// Tests the <see cref="ObjectFactory.Register(System.Type, System.Type)"/> method.
        /// Checks if the registered types are correctly mapped in the <see cref="ObjectFactory.Configuration"/>.
        /// </summary>
        [TestMethod]
        public void TestRegister()
        {
            ObjectFactory.Register(typeof(DAL.Services.ITestDAO), typeof(DAL.Services.Mocked.TestDAO));

            var ifName = typeof(DAL.Services.ITestDAO).FullName;
            var clsName = typeof(DAL.Services.Mocked.TestDAO).FullName;

            Assert.IsNotNull(ifName);
            Assert.IsNotNull(clsName);
            Assert.IsTrue(ObjectFactory.Configuration.InterfaceToImplementationMap.ContainsKey(ifName));
            Assert.AreEqual(clsName, ObjectFactory.Configuration.InterfaceToImplementationMap[ifName]);
        }

        /// <summary>
        /// Tests the <see cref="ObjectFactory.IsRegistered(System.Type)"/> method.
        /// </summary>
        [TestMethod]
        public void TestIsRegistered()
        {
            var actualNotRegisteredYet = ObjectFactory.IsRegistered(typeof(DAL.Services.ITestDAO));

            Assert.IsFalse(actualNotRegisteredYet);

            ObjectFactory.Register(typeof(DAL.Services.ITestDAO), typeof(DAL.Services.Mocked.TestDAO));

            var actualAfterRegister = ObjectFactory.IsRegistered(typeof(DAL.Services.ITestDAO));

            Assert.IsTrue(actualAfterRegister);
        }

        /// <summary>
        /// Tests the <see cref="ObjectFactory.CreateInstance{T}(object[])"/> method.
        /// Checks of the returned instance's type equals the type configured in <see cref="CM.Configurator"/>
        /// </summary>
        [TestMethod]
        public void TestCreateInstace()
        {
            ObjectFactory.Register(typeof(DAL.Services.ITestDAO), typeof(DAL.Services.Mocked.TestDAO));

            var testDAO1 = ObjectFactory.CreateInstance<DAL.Services.ITestDAO>();
            var testDAO2 = ObjectFactory.CreateInstance<DAL.Services.ITestDAO>();

            Assert.IsNotNull(testDAO1);
            Assert.IsNotNull(testDAO2);
            Assert.AreNotSame(testDAO1, testDAO2);
        }

        /// <summary>
        /// Tests the <see cref="ObjectFactory.RegisterSingleton{T}(T)"/> and <see cref="ObjectFactory.GetInstance{T}"/> methods.
        /// Checks of the returned instances of subsequent calls to <see cref="ObjectFactory.GetInstance{T}()"/> are the same.
        /// </summary>
        [TestMethod]
        public void TestRegisterSingletonAndGetInstace()
        {
            ObjectFactory.Register(typeof(DAL.Services.ITestDAO), typeof(DAL.Services.Mocked.TestDAO));

            var testDAO = ObjectFactory.CreateInstance<DAL.Services.ITestDAO>();

            ObjectFactory.RegisterSingleton(testDAO as DAL.Services.ITestDAO);

            var testDAO1 = ObjectFactory.GetInstance<DAL.Services.ITestDAO>();
            var testDAO2 = ObjectFactory.GetInstance<DAL.Services.ITestDAO>();

            Assert.IsNotNull(testDAO1);
            Assert.IsNotNull(testDAO2);
            Assert.AreSame(testDAO1, testDAO2);
        }

        /// <summary>
        /// Tests the <see cref="ObjectFactory.GetInstance{T}(System.Type)"/> method.
        /// Checks if an instance with a given interface can be cast to its base interface
        /// </summary>
        [TestMethod]
        public void TestGetInstance()
        {
            ObjectFactory.Register(typeof(DAL.Services.ITestDAO), typeof(DAL.Services.Mocked.TestDAO));

            var testDAO = ObjectFactory.CreateInstance<DAL.Services.ITestDAO>();

            ObjectFactory.RegisterSingleton(testDAO as DAL.Services.ITestDAO);

            var testDAO1 = ObjectFactory.GetInstance<DAL.Services.IDataAccessObject>(typeof(DAL.Services.ITestDAO));

            Assert.IsNotNull(testDAO1);
        }
    }
}
