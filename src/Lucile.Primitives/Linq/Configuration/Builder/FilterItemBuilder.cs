using System;
using System.Runtime.Serialization;
using Lucile.Json;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(FilterItemGroupBuilder))]
    [KnownType(typeof(BooleanFilterItemBuilder))]
    [KnownType(typeof(StringFilterItemBuilder))]
    [KnownType(typeof(DateTimeFilterItemBuilder))]
    [KnownType(typeof(NumericFilterItemBuilder))]
    [KnownType(typeof(GuidFilterItemBuilder))]
    [KnownType(typeof(AnyFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(101, typeof(FilterItemGroupBuilder))]
    [ProtoBuf.ProtoInclude(102, typeof(BooleanFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(104, typeof(StringFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(105, typeof(DateTimeFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(106, typeof(NumericFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(107, typeof(AnyFilterItemBuilder))]
    [ProtoBuf.ProtoInclude(108, typeof(GuidFilterItemBuilder))]
    [ProtoBuf.ProtoContract(AsReferenceDefault = true)]
    [JsonConverter(typeof(JsonInheritanceConverter), "type")]
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
                result = new StringFilterItemBuilder();
            }
            else if (item is DateTimeBinaryFilterItem)
            {
                result = new DateTimeFilterItemBuilder();
            }
            else if (item is NumericBinaryFilterItem)
            {
                result = new NumericFilterItemBuilder();
            }
            else if (item is AnyFilterItem)
            {
                result = new AnyFilterItemBuilder();
            }
            else if (item is GuidBinaryFilterItem)
            {
                result = new GuidFilterItemBuilder();
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