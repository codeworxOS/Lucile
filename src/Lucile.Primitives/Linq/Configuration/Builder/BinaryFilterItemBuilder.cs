using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(StringBinaryFilterItemBuilder))]
    [KnownType(typeof(DateTimeBinaryFilterItemBuilder))]
    [KnownType(typeof(NumericBinaryFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(101, typeof(StringBinaryFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(102, typeof(DateTimeBinaryFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(103, typeof(NumericBinaryFilterItemBuilder))]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public abstract class BinaryFilterItemBuilder : FilterItemBuilder
    {
        [DataMember(Order = 1)]
        public ValueExpressionBuilder Left { get; set; }

        [DataMember(Order = 2)]
        public ValueExpressionBuilder Right { get; set; }

        public override void LoadFrom(FilterItem value)
        {
            var binary = Get<BinaryFilterItem>(value);
            Left = ValueExpressionBuilder.GetBuilder(binary.Left);
            Right = ValueExpressionBuilder.GetBuilder(binary.Right);
        }

        protected abstract void LoadDetailFrom(BinaryFilterItem value);
    }
}