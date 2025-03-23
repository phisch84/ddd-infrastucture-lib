namespace com.schoste.ddd.Infrastructure.V1.DAL.Services
{
    using V1.DAL.Models;

    /// <summary>
    /// V1 interface to the <see cref="Mocked.TestDAO"/> class
    /// </summary>
    public interface ITestDAO : IDataAccessObject<TestDO, long>
    { }
}
