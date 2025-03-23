namespace com.schoste.ddd.Infrastructure.V1.DAL.Models
{
    /// <summary>
    /// V1 implmenentation of the abstract <see cref="DataObject{ID}"/> class
    /// for testing purposes
    /// </summary>
    public class TestDO : DataObject<long>
    {
        internal string? ExampleLazyLoadPropertyInternal = null;

        public string? ExampleLazyLoadProperty
        {
            set { ExampleLazyLoadPropertyInternal = value; }

            get
            {
                if (ReferenceEquals(ExampleLazyLoadPropertyInternal, null))
                {
                    var value = this.LoadAsync(nameof(ExampleLazyLoadProperty)).Result;

                    this.ExampleLazyLoadProperty = value.ToString();
                }

                return ExampleLazyLoadPropertyInternal;
            }
        }

        public string ExampleProperty { get; set; }

        public TestDO(long id) : base(id)
        {
            this.ExampleProperty = string.Empty;
        }
    }
}
