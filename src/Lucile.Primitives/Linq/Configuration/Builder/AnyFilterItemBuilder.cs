using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class AnyFilterItemBuilder : FilterItemBuilder
    {
        [DataMember(Order = 2)]
        public FilterItemBuilder Filter { get; set; }

        [DataMember(Order = 1)]
        public string Path { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var any = Get<AnyFilterItem>(value);
            this.Path = any.Path.Path;
            this.Filter = GetBuilder(any.Filter);
        }

        protected override FilterItem BuildTarget()
        {
            return new AnyFilterItem(new PathValueExpression(this.Path), this.Filter.Build());
        }
    }
}