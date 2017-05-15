using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class NumericFilterItemBuilder : FilterItemBuilder
    {
        [DataMember(Order = 1)]
        public ValueExpressionBuilder Left { get; set; }

        [DataMember(Order = 3)]
        public RelationalCompareOperator Operator { get; set; }

        [DataMember(Order = 2)]
        public ValueExpressionBuilder Right { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var binary = Get<BinaryFilterItem>(value);
            Left = ValueExpressionBuilder.GetBuilder(binary.Left);
            Right = ValueExpressionBuilder.GetBuilder(binary.Right);

            var numericBinary = Get<NumericBinaryFilterItem>(value);
            Operator = numericBinary.Operator;
        }

        public override FilterItem ToTarget()
        {
            return new NumericBinaryFilterItem(Left.ToTarget(), Right?.ToTarget(), Operator);
        }
    }
}