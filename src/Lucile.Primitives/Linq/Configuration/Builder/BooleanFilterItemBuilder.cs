using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class BooleanFilterItemBuilder : FilterItemBuilder
    {
        [DataMember(Order = 1)]
        public BooleanOperator Operator { get; set; }

        [DataMember(Order = 2)]
        public ValueExpressionBuilder Value { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var boolean = Get<BooleanFilterItem>(value);
            Operator = boolean.Operator;
            Value = ValueExpressionBuilder.GetBuilder(boolean.Value);
        }

        public override FilterItem ToTarget()
        {
            return new BooleanFilterItem(Value.ToTarget(), Operator);
        }
    }
}