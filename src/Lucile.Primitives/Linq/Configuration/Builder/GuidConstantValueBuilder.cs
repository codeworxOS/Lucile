using System;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public class GuidConstantValueBuilder : ValueExpressionBuilder
    {
        [DataMember(Order = 1)]
        public Guid Value { get; set; }

        public override void LoadFrom(ValueExpression value)
        {
            this.Value = ((GuidConstantValue)value).Value;
        }

        protected override ValueExpression BuildTarget()
        {
            return new GuidConstantValue(this.Value);
        }
    }
}