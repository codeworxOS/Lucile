using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class StringBinaryFilterItemBuilder : BinaryFilterItemBuilder
    {
        [DataMember(Order = 1)]
        public StringOperator Operator { get; set; }

        public override FilterItem ToTarget()
        {
            return new StringBinaryFilterItem(Left.ToTarget(), Right.ToTarget(), Operator);
        }

        protected override void LoadDetailFrom(BinaryFilterItem value)
        {
            var stringBinary = Get<StringBinaryFilterItem>(value);
            Operator = stringBinary.Operator;
        }
    }
}