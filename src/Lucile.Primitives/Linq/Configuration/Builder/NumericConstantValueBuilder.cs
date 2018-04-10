using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class NumericConstantValueBuilder : ValueExpressionBuilder
    {
        [DataMember(Order = 1)]
        public decimal Value { get; set; }

        public override void LoadFrom(ValueExpression value)
        {
            this.Value = ((NumericConstantValue)value).Value;
        }

        protected override ValueExpression BuildTarget()
        {
            return new NumericConstantValue(this.Value);
        }
    }
}