using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class DateTimeBinaryFilterItemBuilder : BinaryFilterItemBuilder
    {
        [DataMember(Order = 1)]
        public RelationalCompareOperator Operator { get; set; }

        public override FilterItem ToTarget()
        {
            return new DateTimeBinaryFilterItem(Left.ToTarget(), Right.ToTarget(), Operator);
        }

        protected override void LoadDetailFrom(BinaryFilterItem value)
        {
            var stringBinary = Get<DateTimeBinaryFilterItem>(value);
            Operator = stringBinary.Operator;
        }
    }
}