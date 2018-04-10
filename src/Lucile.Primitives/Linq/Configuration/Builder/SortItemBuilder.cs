using System.Runtime.Serialization;
using Lucile.Builder;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class SortItemBuilder : BaseBuilder<SortItem>
    {
        [DataMember(Order = 1)]
        public string PropertyPath { get; set; }

        [DataMember(Order = 2)]
        public SortDirection SortDirection { get; set; }

        public override void LoadFrom(SortItem value)
        {
            this.PropertyPath = value.PropertyPath;
            this.SortDirection = value.SortDirection;
        }

        protected override SortItem BuildTarget()
        {
            return new SortItem(this.PropertyPath, this.SortDirection);
        }
    }
}