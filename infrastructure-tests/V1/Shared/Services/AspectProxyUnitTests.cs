using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace com.schoste.ddd.Infrastructure.V1.Shared.Services
{
    using Aspects;
    using Shared.Exceptions;

    /// <summary>
    /// Test implementation of the <see cref="IMethodAspect"/> interface on an attribute to test the correct
    /// functionality of <see cref="AspectProxy{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TestMethodAspect : Attribute, IMethodAspect
    {
        static public Queue<ExpectedParametersModel> EPMsToAssertBeforeMethodCall = new Queue<ExpectedParametersModel>();
        static public Queue<ExpectedParametersModel> EPMsToAssertAfterMethodCall = new Queue<ExpectedParametersModel>();
        static public Queue<ExpectedParametersModel> PMsBeforeMethodCall = new Queue<ExpectedParametersModel>();
        static public Queue<ExpectedParametersModel> PMsAfterMethodCall = new Queue<ExpectedParametersModel>();

        virtual public void ValidateMethod(MethodInfo method)
        {
            var returnType = method.ReturnType;
            var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            var typesToCheck = parameterTypes.ToList();
                typesToCheck.Add(returnType);

            foreach (var typeToCheck in typesToCheck)
            {
                if (typeof(void).Equals(typeToCheck)) continue;
                
                var serializableAttr = typeToCheck.GetCustomAttribute(typeof(SerializableAttribute));

                if (ReferenceEquals(null, serializableAttr)) throw new InvalidMethodForAspectException(method, this);                
            }
        }

        virtual public void BeforeMethodCall(object? target, object?[]? args, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        {
            PMsBeforeMethodCall.Enqueue(new ExpectedParametersModel()
            {
                ExpectedArgs = args,
                ExpectedException = null,
                ExpectedImplementingMethod = implementingMethod,
                ExpectedInterfaceType = interfaceType,
                ExpectedReturnValue = null,
                ExpectedTarget = target,
            });

            if (!EPMsToAssertBeforeMethodCall.TryDequeue(out var epm)) return;
            if (epm == null) return;

            // the expected target will be a proxied instance, so this check makes no sense
            //if (epm.ExpectedTarget == null) Assert.IsNull(target);
            //else Assert.AreEqual(epm.ExpectedTarget, target);

            if (epm.ExpectedArgs == null)
            {
                Assert.IsInstanceOfType<object[]>(args);
                Assert.AreEqual(0, args.Length);
            }
            else Assert.AreEqual(epm.ExpectedArgs, args);

            if (epm.ExpectedInterfaceType == null) Assert.IsNull(interfaceType);
            else Assert.AreEqual(epm.ExpectedInterfaceType, interfaceType);

            if (epm.ExpectedImplementingMethod == null) Assert.IsNull(implementingMethod);
            else Assert.AreEqual(epm.ExpectedImplementingMethod.Name, implementingMethod.Name);
        }

        virtual public void AfterMethodCall(DateTime methodCallTime, TimeSpan methodRunTime, object? target, object?[]? args, ref object? returnValue, ref Exception? ex, Type? interfaceType = null, MethodInfo? implementingMethod = null)
        {
            PMsAfterMethodCall.Enqueue(new ExpectedParametersModel()
            {
                ExpectedArgs = args,
                ExpectedException = (ex == null) ? null : ex.GetType(),
                ExpectedImplementingMethod = implementingMethod,
                ExpectedInterfaceType = interfaceType,
                ExpectedReturnValue = returnValue,
                ExpectedTarget = target,
            });

            if (!EPMsToAssertAfterMethodCall.TryDequeue(out var epm)) return;
            if (epm == null) return;

            // the expected target will be a proxied instance, so this check makes no sense
            //if (epm.ExpectedTarget == null) Assert.IsNull(target);
            //else Assert.AreEqual(epm.ExpectedTarget, target);

            if (epm.ExpectedArgs == null)
            {
                Assert.IsInstanceOfType<object[]>(args);
                Assert.AreEqual(0, args.Length);
            }
            else Assert.AreEqual(epm.ExpectedArgs, args);

            if (epm.ExpectedInterfaceType == null) Assert.IsNull(interfaceType);
            else Assert.AreEqual(epm.ExpectedInterfaceType, interfaceType);

            if (epm.ExpectedImplementingMethod == null) Assert.IsNull(implementingMethod);
            else Assert.AreEqual(epm.ExpectedImplementingMethod.Name, implementingMethod.Name);

            if (epm.ExpectedReturnValue == null) Assert.IsNull(returnValue);
            else Assert.AreEqual(epm.ExpectedReturnValue, returnValue);

            if (epm.ExpectedException == null) Assert.IsNull(ex);
            else
            {
                Assert.IsNotNull(ex);
                Assert.AreEqual(epm.ExpectedException, ex.GetType());
            }
        }
    }

    /// <summary>
    /// Data model of parameters expected to be passed in
    /// <see cref="TestMethodAspect.BeforeMethodCall(object?, object?[]?, Type?, MethodInfo?)"/> and
    /// <see cref="TestMethodAspect.AfterMethodCall(DateTime, TimeSpan, object?, object?[]?, ref object?, ref Exception?, Type?, MethodInfo?)"/>
    /// for assertation.
    /// </summary>
    public class ExpectedParametersModel()
    {
        public object? ExpectedTarget;
        public object?[]? ExpectedArgs;
        public Type? ExpectedInterfaceType;
        public MethodInfo? ExpectedImplementingMethod;
        public object? ExpectedReturnValue;
        public Type? ExpectedException;
    }

    /// <summary>
    /// Interface to the test class <see cref="TestClass"/> which's methods will be decorated by
    /// <see cref="AspectProxy{T}"/> with the <see cref="TestMethodAspect"/>.
    /// </summary>
    public interface ITestClass
    {
        void TM_VoidNoException();

        void TM_VoidWithException();

        string TM_StringNoException(int param);

        Task<int> TM_IntNoExceptionAsync();

        void TM_VoidNoExceptionAspectOnClass();

        [TestMethodAspect]
        void TM_VoidNoExceptionAspectOnInterface();

        [TestMethodAspect]
        Task<int> TM_IntInvalidMethodForAspectException();

        [TestMethodAspect]
        void TM_VoidInvalidMethodForAspectException(Task<int> parameter);
    }

    /// <summary>
    /// Implementation of <see cref="ITestClass"/>.
    /// Provides the logic in its methods to conduct the tests specified in <see cref="AspectProxyUnitTests"/>.
    /// </summary>
    public class TestClass : ITestClass
    {
        public void TM_VoidNoException()
        {
            return;
        }

        public void TM_VoidWithException()
        {
            throw new NotImplementedException();
        }

        public string TM_StringNoException(int param)
        {
            return param.ToString();
        }

        public async Task<int> TM_IntNoExceptionAsync()
        {
            return await Task.Run(() => { return 0; });
        }

        [TestMethodAspect]
        public void TM_VoidNoExceptionAspectOnClass()
        {
            return;
        }

        public void TM_VoidNoExceptionAspectOnInterface()
        {
            return;
        }

        public async Task<int> TM_IntInvalidMethodForAspectException()
        {
            return await Task.Run(() => { return 0; });
        }

        public void TM_VoidInvalidMethodForAspectException(Task<int> parameter)
        {
            return;
        }
    }

    /// <summary>
    /// Suite of tests to assert the correct implementation of <see cref="AspectProxy{T}"/>
    /// </summary>
    [TestClass]
    [TestCategory("Unit Test")]
    public class AspectProxyUnitTests
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
        /// Initializes the test class and prepares the test setup.
        /// </summary>
        /// <param name="testContext">The context provided by the test runner</param>
        [ClassInitialize]
        static public void Initialize(TestContext testContext)
        {
            testContextInstance = testContext;

            ObjectFactory.Register(typeof(ITestClass), typeof(TestClass)); // Configure test class
        }

        /// <summary>
        /// Resets the test set up before any test method is called.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            TestMethodAspect.EPMsToAssertBeforeMethodCall.Clear();
            TestMethodAspect.EPMsToAssertAfterMethodCall.Clear();
            TestMethodAspect.PMsBeforeMethodCall.Clear();
            TestMethodAspect.PMsAfterMethodCall.Clear();
        }

        /// <summary>
        /// Tests if a call to a method that expects no parameters and returns void is handled as expected.
        /// Executes <see cref="ITestClass.TM_VoidNoException"/>
        /// </summary>
        [TestMethod]
        public void TestCall_TM_VoidNoException()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = null,
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_VoidNoException)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = null,
                ExpectedException = null,
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            tcInstance.TM_VoidNoException();
        }

        /// <summary>
        /// Tests if a call to a method that expects no parameters, that is supposed to return void, but throws an exception
        /// is handled as expected.
        /// Executes <see cref="ITestClass.TM_VoidWithException"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestCall_TM_VoidWithException()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = null,
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_VoidWithException)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = null,
                ExpectedException = typeof(NotImplementedException),
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            tcInstance.TM_VoidWithException();
        }

        /// <summary>
        /// Tests if a call to a method that expects an integer as parameter and returns a string is handled as expected.
        /// Executes <see cref="ITestClass.TM_StringNoException(int)"/>
        /// </summary>
        [TestMethod]
        public void TestCall_TM_StringNoException()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = [3],
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_StringNoException)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = "3",
                ExpectedException = null,
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            var param = Convert.ToInt32(expectedParameters.ExpectedArgs[0]);
            var actualReturn = tcInstance.TM_StringNoException(param);

            Assert.AreEqual(expectedParameters.ExpectedReturnValue.ToString(), actualReturn);
        }

        /// <summary>
        /// Tests if a call to an asynchronous method that expects no parameters and returns an integer
        /// is handled as expected.
        /// Executes <see cref="ITestClass.TM_IntNoExceptionAsync"/>
        /// </summary>
        [TestMethod]
        public void TestCall_TM_IntNoExceptionAsync()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = null,
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_IntNoExceptionAsync)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = null,
                ExpectedException = null,
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            var actualReturn = tcInstance.TM_IntNoExceptionAsync().Result;

            Assert.AreEqual(Convert.ToInt32(expectedParameters.ExpectedReturnValue), actualReturn);
        }

        /// <summary>
        /// Tests if a call to a method that expects no parameters and returns void is handled as expected.
        /// Executes <see cref="ITestClass.TM_VoidNoExceptionAspectOnClass"/>
        /// </summary>
        [TestMethod]
        public void TestCall_TM_VoidNoExceptionAspectOnClass()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = null,
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_VoidNoExceptionAspectOnClass)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = null,
                ExpectedException = null,
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            tcInstance.TM_VoidNoExceptionAspectOnClass();

            Assert.IsTrue(TestMethodAspect.PMsBeforeMethodCall.Count > 0);
            Assert.IsTrue(TestMethodAspect.PMsAfterMethodCall.Count > 0);
        }

        /// <summary>
        /// Tests if a call to a method that expects no parameters and returns void is handled as expected.
        /// Executes <see cref="ITestClass.TM_VoidNoExceptionAspectOnInterface"/>
        /// </summary>
        [TestMethod]
        public void TestCall_TM_VoidNoExceptionAspectOnInterface()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();
            var expectedParameters = new ExpectedParametersModel()
            {
                ExpectedTarget = tcInstance,
                ExpectedArgs = null,
                ExpectedImplementingMethod = typeof(TestClass).GetMethod(nameof(TestClass.TM_VoidNoExceptionAspectOnInterface)),
                ExpectedInterfaceType = typeof(ITestClass),
                ExpectedReturnValue = null,
                ExpectedException = null,
            };

            TestMethodAspect.EPMsToAssertBeforeMethodCall.Enqueue(expectedParameters);
            TestMethodAspect.EPMsToAssertAfterMethodCall.Enqueue(expectedParameters);

            tcInstance.TM_VoidNoExceptionAspectOnInterface();

            Assert.IsTrue(TestMethodAspect.PMsBeforeMethodCall.Count > 0);
            Assert.IsTrue(TestMethodAspect.PMsAfterMethodCall.Count > 0);
        }

        /// <summary>
        /// Tests if adding the aspect to an incompatible method (invalid return type) triggers <see cref="InvalidMethodForAspectException"/>
        /// Executes <see cref="ITestClass.TM_IntInvalidMethodForAspectException"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidMethodForAspectException))]
        public void TestCall_TM_IntInvalidMethodForAspectException()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();

            tcInstance.TM_IntInvalidMethodForAspectException();
        }

        /// <summary>
        /// Tests if adding the aspect to an incompatible method (invalid parameter) triggers <see cref="InvalidMethodForAspectException"/>
        /// Executes <see cref="ITestClass.TM_VoidInvalidMethodForAspectException"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidMethodForAspectException))]
        public void TestCall_TM_VoidInvalidMethodForAspectException()
        {
            var tcInstance = ObjectFactory.CreateInstance<ITestClass>();

            tcInstance.TM_VoidInvalidMethodForAspectException(new Task<int>(() => { return 0; }));
        }
    }
}
