using System;
using System.Collections.Generic;
using System.Linq;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Services
{
    using DAL.Models;
    using DAL.Services.Mocked;
    using Shared.Services;

    /// <summary>
    /// V1 unit tests of the <see cref="DataAccessObject{DO, ID}"/> class
    /// </summary>
    [TestClass]
    public class TestDAOUnitTests
    {
        protected ITestDAO TestDAO = ObjectFactory.CreateInstance<ITestDAO>();

        protected IDictionary<long, long> DOCreatedSequence = new Dictionary<long, long>();

        protected bool BeforeLoadCalled = false;
        protected bool AfterLoadCalled = false;

        protected HashSet<TestDO> DOsExpectedToBeSaved = new HashSet<TestDO>();

        protected IDictionary<long, TestDO> DOsExpectedToBeGotten = new Dictionary<long, TestDO>();

        protected HashSet<TestDO> DOsExpectedToBeDeleted = new HashSet<TestDO>();

        private void onBeforeCreateDataObject(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.Ids);

            var ids = e.Ids.ToList();

            Assert.AreEqual(1, ids.Count);

            DOCreatedSequence[DateTime.Now.Ticks] = ids[0];
        }

        private void onAfterCreateDataObject(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e);
            Assert.IsNotNull(e.Ids);
            Assert.IsNotNull(e.DataObjects);

            var ids = e.Ids.ToList();
            var dataObjects = e.DataObjects.ToList();

            Assert.AreEqual(1, ids.Count);
            Assert.AreEqual(1, dataObjects.Count);
            Assert.AreEqual(ids[0], dataObjects[0].Id);

            DOCreatedSequence[DateTime.Now.Ticks] = ids[0];
        }

        private void onBeforeLoad(object? sender, LoadEventArgs<TestDO, long> e)
        {
            BeforeLoadCalled = true;
        }

        private void onAfterLoad(object? sender, LoadEventArgs<TestDO, long> e)
        {
            AfterLoadCalled = true;
        }

        private void onBeforeSaveDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.DataObjects);

            var dosToBeSaved = e.DataObjects.ToList();

            Assert.IsTrue(dosToBeSaved.All(DOsExpectedToBeSaved.Contains));
            Assert.IsTrue(DOsExpectedToBeSaved.All(dosToBeSaved.Contains));
        }

        private void onAfterSaveDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.DataObjects);

            var dosToBeSaved = e.DataObjects.ToList();

            Assert.IsTrue(dosToBeSaved.All(DOsExpectedToBeSaved.Contains));
            Assert.IsTrue(DOsExpectedToBeSaved.All(dosToBeSaved.Contains));

        }

        private void onBeforeGetDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.Ids);
            Assert.IsNull(e.DataObjects); // No data objects before they were loaded -> NULL

            if (!e.Ids.Any()) return; // No id(s) specified -> Get() should load all available ones

            var dosToBeGotten = e.Ids.ToList();

            Assert.IsTrue(dosToBeGotten.All(DOsExpectedToBeGotten.Keys.Contains));
            Assert.IsTrue(DOsExpectedToBeGotten.Keys.All(dosToBeGotten.Contains));
        }

        private void onAfterGetDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.Ids);
            Assert.IsNotNull(e.DataObjects);

            // No id(s) were specified and no dataobjects were found
            if (!e.Ids.Any() && !e.DataObjects.Any()) return;

            var dosToBeGotten = e.Ids.ToList();

            Assert.IsTrue(dosToBeGotten.All(DOsExpectedToBeGotten.Keys.Contains));
            Assert.IsTrue(DOsExpectedToBeGotten.Keys.All(dosToBeGotten.Contains));

            foreach (var id in DOsExpectedToBeGotten.Keys)
            {
                var expectedDO = DOsExpectedToBeGotten[id];
                var actualDO = e.DataObjects.FirstOrDefault(dataObj => dataObj.Id == id);

                Assert.IsNotNull(actualDO);
                Assert.AreEqual(expectedDO, actualDO);
            }
        }

        private void onBeforeDeleteDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.DataObjects);

            var dosToBeDeleted = e.DataObjects.ToList();

            Assert.IsTrue(dosToBeDeleted.All(DOsExpectedToBeDeleted.Contains));
            Assert.IsTrue(DOsExpectedToBeDeleted.All(dosToBeDeleted.Contains));
        }

        private void onAfterDeleteDataObjects(object? sender, DataObjectEventArgs<TestDO, long> e)
        {
            Assert.IsNotNull(e.Ids);

            var doIdsToBeDeleted = e.Ids.ToList();
            var doIdsExpectedToBeDeleted = DOsExpectedToBeDeleted.Select(dataObj => dataObj.Id).ToList();

            Assert.IsTrue(doIdsToBeDeleted.All(doIdsExpectedToBeDeleted.Contains));
            Assert.IsTrue(doIdsExpectedToBeDeleted.All(doIdsToBeDeleted.Contains));
        }

        /// <summary>
        /// Initializes static properties of the class
        /// </summary>
        [ClassInitialize]
        static public void SetUp(TestContext testContext)
        {
            var ifName = typeof(ITestDAO).FullName;
            var clsName = typeof(TestDAO).FullName;

            ObjectFactory.Configuration.InterfaceToImplementationMap[ifName] = clsName;
        }

        /// <summary>
        /// Resets states and properties of the test class before each test method is executed
        /// </summary>
        [TestInitialize]
        public void Reset()
        {
            DOCreatedSequence.Clear();
            DOsExpectedToBeSaved.Clear();
            DOsExpectedToBeGotten.Clear();
            DOsExpectedToBeDeleted.Clear();

            BeforeLoadCalled = false;
            AfterLoadCalled = false;
        }

        /// <summary>
        /// Tests the implementation of <see cref="IDataAccessObject{DO, ID}.Create(ID, object[])"/>.
        /// Ensures that a <see cref="DataObject{ID}"/> instance with the given id is created, and
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeCreateDataObject"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterCreateDataObject"/>
        /// are called properly.
        /// </summary>
        [TestMethod]
        public void TestCreate()
        {
            TestDAO.BeforeCreateDataObject += onBeforeCreateDataObject;
            TestDAO.AfterCreateDataObject += onAfterCreateDataObject;

            var dataObject = TestDAO.Create(0);

            Assert.AreEqual(2, DOCreatedSequence.Count);
            Assert.IsTrue(DOCreatedSequence.Values.All(v => v == 0));
            Assert.IsNotNull(dataObject);
            Assert.AreEqual(0, dataObject.Id);
        }

        /// <summary>
        /// Tests the implementation of <see cref="IDataAccessObject{DO, ID}.CreateAsync(ID, object[])"/>.
        /// Ensures that a <see cref="DataObject{ID}"/> instance with the given id is created, and
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeCreateDataObject"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterCreateDataObject"/>
        /// are called properly.
        /// </summary>
        [TestMethod]
        public void TestCreateAsync()
        {
            TestDAO.BeforeCreateDataObject += onBeforeCreateDataObject;
            TestDAO.AfterCreateDataObject += onAfterCreateDataObject;

            var dataObject = TestDAO.CreateAsync(0).Result;

            Assert.AreEqual(2, DOCreatedSequence.Count);
            Assert.IsTrue(DOCreatedSequence.Values.All(v => v == 0));
            Assert.IsNotNull(dataObject);
            Assert.AreEqual(0, dataObject.Id);
        }

        /// <summary>
        /// Tests the logic between <see cref="DataObject{ID}.LoadAsync(object[])"/>
        /// and <see cref="DataAccessObject{DO, ID}.LoadInternal(object[])"/> for lazy loading of properties.
        /// Ensures the requested property is loaded into the data object, and
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeLoad"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterLoad"/>
        /// are called properly
        /// </summary>
        [TestMethod]
        public void TestLoad()
        {
            TestDAO.BeforeLoad += onBeforeLoad;
            TestDAO.AfterLoad += onAfterLoad;

            var dataObject = TestDAO.Create(0);

            Assert.AreEqual("value", dataObject.ExampleLazyLoadProperty);
            Assert.IsTrue(BeforeLoadCalled);
            Assert.IsTrue(AfterLoadCalled);
        }

        /// <summary>
        /// Tests the correct implementation of <see cref="IDataAccessObject{DO, ID}.Save(DO)"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.Get()"/>.
        /// Ensures that a data object that is already present or that is saved via <see cref="IDataAccessObject{DO, ID}.Save(DO)"/>
        /// to the storage can be obtained using <see cref="IDataAccessObject{DO, ID}.Get()"/>.
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterGetDataObjects"/>
        /// are called properly.
        /// </summary>
        [TestMethod]
        public void TestSaveAndGet()
        {
            TestDAO.BeforeSaveDataObjects += onBeforeSaveDataObjects;
            TestDAO.AfterSaveDataObjects += onAfterSaveDataObjects;
            TestDAO.BeforeGetDataObjects += onBeforeGetDataObjects;
            TestDAO.AfterGetDataObjects += onAfterGetDataObjects;

            DOsExpectedToBeGotten[long.MaxValue] = null;

            var dataObjects = TestDAO.Get(new[] { long.MaxValue });

            Assert.IsNotNull(dataObjects);
            Assert.AreEqual(0, dataObjects.Count());

            var dataObject = TestDAO.Create(1);

            DOsExpectedToBeSaved.Add(dataObject);

            TestDAO.Save(dataObject);

            DOsExpectedToBeGotten.Clear();
            DOsExpectedToBeGotten[dataObject.Id] = dataObject;

            var gotDataObjects = TestDAO.Get();

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(1, gotDataObjects.Count());
            Assert.AreEqual(dataObject.Id, gotDataObjects.First().Id);
        }

        /// <summary>
        /// Tests the correct implementation of <see cref="IDataAccessObject{DO, ID}.SaveAsync(DO)"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.GetAsync()"/>.
        /// Ensures that a data object that is already present or that is saved via <see cref="IDataAccessObject{DO, ID}.SaveAsync(DO)"/>
        /// to the storage can be obtained using <see cref="IDataAccessObject{DO, ID}.GetAsync()"/>.
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterGetDataObjects"/>
        /// are called properly.
        /// </summary>
        [TestMethod]
        public void TestSaveAndGetAsync()
        {
            TestDAO.BeforeSaveDataObjects += onBeforeSaveDataObjects;
            TestDAO.AfterSaveDataObjects += onAfterSaveDataObjects;
            TestDAO.BeforeGetDataObjects += onBeforeGetDataObjects;
            TestDAO.AfterGetDataObjects += onAfterGetDataObjects;

            DOsExpectedToBeGotten[long.MaxValue] = null;

            var dataObjects = TestDAO.Get(new[] { long.MaxValue });

            Assert.IsNotNull(dataObjects);
            Assert.AreEqual(0, dataObjects.Count());

            var dataObject = TestDAO.CreateAsync(1).Result;

            DOsExpectedToBeSaved.Add(dataObject);

            TestDAO.SaveAsync(dataObject).Wait();

            DOsExpectedToBeGotten.Clear();
            DOsExpectedToBeGotten[dataObject.Id] = dataObject;

            var gotDataObjects = TestDAO.GetAsync().Result;

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(1, gotDataObjects.Count());
            Assert.AreEqual(dataObject.Id, gotDataObjects.First().Id);
        }

        /// <summary>
        /// Tests the correct implementation of <see cref="IDataAccessObject{DO, ID}.Save(DO)"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.Get()"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.Delete(DO)"/>.
        /// Ensures that a data object that is already present or that is deleted via <see cref="IDataAccessObject{DO, ID}.Delete(DO)"/>
        /// from the storage cannot be obtained using <see cref="IDataAccessObject{DO, ID}.Get()"/>.
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeDeleteDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterDeleteDataObjects"/>
        /// are called properly.
        [TestMethod]
        public void TestSaveAndGetAndDelete()
        {
            var dataObject = TestDAO.Create(1);

            TestDAO.Save(dataObject);

            var gotDataObjects = TestDAO.Get();

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(1, gotDataObjects.Count());
            Assert.AreEqual(dataObject.Id, gotDataObjects.First().Id);

            DOsExpectedToBeDeleted.Add(dataObject);

            TestDAO.BeforeDeleteDataObjects += onBeforeDeleteDataObjects;
            TestDAO.AfterDeleteDataObjects += onAfterDeleteDataObjects;
            TestDAO.Delete(dataObject);

            var gotAfterDeleteDataObject = TestDAO.Get();

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(0, gotDataObjects.Count());
        }

        /// <summary>
        /// Tests the correct implementation of <see cref="IDataAccessObject{DO, ID}.SaveAsync(DO)"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.GetAsync()"/>,
        /// and the correct implementation of <see cref="IDataAccessObject{DO, ID}.DeleteAsync(DO)"/>.
        /// Ensures that a data object that is already present or that is deleted via <see cref="IDataAccessObject{DO, ID}.DeleteAsync(DO)"/>
        /// from the storage cannot be obtained using <see cref="IDataAccessObject{DO, ID}.GetAsync()"/>.
        /// ensures that <see cref="IDataAccessObject{DO, ID}.BeforeSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterSaveDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterGetDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.BeforeDeleteDataObjects"/>
        /// and <see cref="IDataAccessObject{DO, ID}.AfterDeleteDataObjects"/>
        /// are called properly.
        [TestMethod]
        public void TestSaveAndGetAndDeleteAsync()
        {
            var dataObject = TestDAO.CreateAsync(1).Result;

            TestDAO.SaveAsync(dataObject).Wait();

            var gotDataObjects = TestDAO.GetAsync().Result;

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(1, gotDataObjects.Count());
            Assert.AreEqual(dataObject.Id, gotDataObjects.First().Id);

            DOsExpectedToBeDeleted.Add(dataObject);

            TestDAO.BeforeDeleteDataObjects += onBeforeDeleteDataObjects;
            TestDAO.AfterDeleteDataObjects += onAfterDeleteDataObjects;
            TestDAO.DeleteAsync(dataObject).Wait();

            var gotAfterDeleteDataObject = TestDAO.GetAsync().Result;

            Assert.IsNotNull(gotDataObjects);
            Assert.AreEqual(0, gotDataObjects.Count());
        }
    }
}