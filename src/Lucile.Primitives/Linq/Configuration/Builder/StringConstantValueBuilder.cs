using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class StringConstantValueBuilder : ValueExpressionBuilder
    {
        [DataMember(Order = 1)]
        public string Value { get; set; }

        public override void LoadFrom(ValueExpression value)
        {
            this.Value = ((StringConstantValue)value).Value;
        }

        protected override ValueExpression BuildTarget()
        {
            return new StringConstantValue(this.Value);
        }
    }
}