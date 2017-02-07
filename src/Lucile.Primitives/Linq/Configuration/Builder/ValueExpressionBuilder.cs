using System;
using System.Runtime.Serialization;
using Lucile.Json;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(PathValueExpressionBuilder))]
    [KnownType(typeof(StringConstantValueBuilder))]
    [KnownType(typeof(NumericConstantValueBuilder))]
    [KnownType(typeof(DateTimeConstantValueBuilder))]
    [ProtoBuf.ProtoInclude(101, typeof(PathValueExpressionBuilder))]
    [ProtoBuf.ProtoInclude(102, typeof(StringConstantValueBuilder))]
    [ProtoBuf.ProtoInclude(103, typeof(NumericConstantValueBuilder))]
    [ProtoBuf.ProtoInclude(104, typeof(DateTimeConstantValueBuilder))]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    [JsonConverter(typeof(JsonInheritanceConverter), "type")]
    public abstract class ValueExpressionBuilder : BaseBuilder<ValueExpression>
    {
        public static ValueExpressionBuilder GetBuilder(ValueExpression item)
        {
            ValueExpressionBuilder result = null;
            if (item is PathValueExpression)
            {
                result = new PathValueExpressionBuilder();
            }
            else if (item is StringConstantValue)
            {
                result = new StringConstantValueBuilder();
            }
            else if (item is NumericConstantValue)
            {
                result = new NumericConstantValueBuilder();
            }
            else if (item is DateTimeConstantValue)
            {
                result = new DateTimeConstantValueBuilder();
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