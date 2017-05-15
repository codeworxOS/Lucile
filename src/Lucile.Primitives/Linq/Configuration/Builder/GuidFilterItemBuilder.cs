using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class GuidFilterItemBuilder : FilterItemBuilder
    {
        [DataMember(Order = 1)]
        public ValueExpressionBuilder Left { get; set; }

        [DataMember(Order = 3)]
        public RelationalCompareOperator Operator { get; set; }

        [DataMember(Order = 2)]
        public ValueExpressionBuilder Right { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var relational = Get<RelationalFilterItem>(value);
            Left = ValueExpressionBuilder.GetBuilder(relational.Left);
            Right = ValueExpressionBuilder.GetBuilder(relational.Right);
            Operator = relational.Operator;
        }

        public override FilterItem ToTarget()
        {
            return new GuidBinaryFilterItem(Left.ToTarget(), Right?.ToTarget(), Operator);
        }
    }
}