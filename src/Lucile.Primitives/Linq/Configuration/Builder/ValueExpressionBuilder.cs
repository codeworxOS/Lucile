using System;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(PathValueExpressionBuilder))]
    [ProtoBuf.ProtoInclude(101, typeof(PathValueExpressionBuilder))]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public abstract class ValueExpressionBuilder : BaseBuilder<ValueExpression>
    {
        public static ValueExpressionBuilder GetBuilder(ValueExpression item)
        {
            ValueExpressionBuilder result = null;
            if (item is PathValueExpression)
            {
                result = new PathValueExpressionBuilder();
            }

            if (result == null)
            {
                throw new NotSupportedException($"Unsupported ValueExpression {item?.GetType()} detected");
            }

            result.LoadFrom(item);
            return result;
        }
    }
}