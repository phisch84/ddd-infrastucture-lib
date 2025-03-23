using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace com.schoste.ddd.Infrastructure.V1.DAL.Services.Mocked
{
    using V1.DAL.Models;

    /// <summary>
    /// V1 mock implementation of the <see cref="DataAccessObject{DO, ID}"/> to test
    /// its correct implementation
    /// </summary>
    public class TestDAO : DataAccessObject<TestDO, long>, ITestDAO
    {
        protected IDictionary<long, TestDO> Storage = new Dictionary<long, TestDO>();

        protected override object LoadInternal(params object[] args)
        {
            Thread.Sleep(1000);
            return "value" as object; 
        }

        protected override TestDO CreateInternal(long id, object[] args)
        {
            return new TestDO(0);
        }

        protected override void SaveInternal(IEnumerable<TestDO> dataObjects)
        {
            foreach (var obj in dataObjects)
            {
                Thread.Sleep(100);

                this.Storage[obj.Id] = obj;
            }
        }

        protected override IEnumerable<TestDO> GetInternal(IEnumerable<long>? ids)
        {
            Thread.Sleep(100);

            if (ReferenceEquals(ids, null)) return this.Storage.Values;

            var result = this.Storage.Values.Where(v => ids.Contains(v.Id));

            return result;
        }

        protected override void DeleteInternal(IEnumerable<TestDO> dataObjects)
        {
            foreach (var obj in dataObjects)
            {
                Thread.Sleep(100);

                this.Storage.Remove(obj.Id);
            }
        }
    }
}
