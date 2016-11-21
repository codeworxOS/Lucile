using System;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(FilterItemGroupBuilder))]
    [KnownType(typeof(BooleanFilterItemBuilder))]
    [KnownType(typeof(BinaryFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(101, typeof(FilterItemGroupBuilder))]
    [ProtoBuf.ProtoInclude(102, typeof(BooleanFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(103, typeof(BinaryFilterItemBuilder))]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    public abstract class FilterItemBuilder : BaseBuilder<FilterItem>
    {
        public static FilterItemBuilder GetBuilder(FilterItem item)
        {
            FilterItemBuilder result = null;
            if (item is FilterItemGroup)
            {
                result = new FilterItemGroupBuilder();
            }
            else if (item is BooleanFilterItem)
            {
                result = new BooleanFilterItemBuilder();
            }
            else if (item is StringBinaryFilterItem)
            {
                result = new StringBinaryFilterItemBuilder();
            }
            else if (item is DateTimeBinaryFilterItem)
            {
                result = new DateTimeBinaryFilterItemBuilder();
            }
            else if (item is NumericBinaryFilterItem)
            {
                result = new NumericBinaryFilterItemBuilder();
            }

            if (result == null)
            {
                throw new NotSupportedException($"Unsupported FilterItem {item?.GetType()} detected");
            }

            result.LoadFrom(item);
            return result;
        }
    }
}