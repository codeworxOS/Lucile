using System;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class DateTimeConstantValueBuilder : ValueExpressionBuilder
    {
        [DataMember(Order = 1)]
        public DateTime Value { get; set; }

        public override void LoadFrom(ValueExpression value)
        {
            this.Value = ((DateTimeConstantValue)value).Value;
        }

        protected override ValueExpression BuildTarget()
        {
            return new DateTimeConstantValue(this.Value);
        }
    }
}