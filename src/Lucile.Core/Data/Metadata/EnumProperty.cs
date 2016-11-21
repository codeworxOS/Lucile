using System;
using System.Linq;
using System.Reflection;
using Lucile.Data.Metadata.Builder;

namespace Lucile.Data.Metadata
{
    public class EnumProperty : ScalarProperty
    {
        internal EnumProperty(EntityMetadata entity, EnumPropertyBuilder builder, bool isPrimaryKey)
            : base(entity, builder, isPrimaryKey)
        {
            EnumType = builder.EnumTypeInfo.ClrType;
            IsFlag = builder.EnumTypeInfo.ClrType.GetTypeInfo().CustomAttributes.OfType<FlagsAttribute>().Any();
            UnderlyingNumericType = builder.UnderlyingNumericType;
        }

        public Type EnumType
        {
            get;
        }

        public bool IsFlag { get; }

        public NumericPropertyType UnderlyingNumericType
        {
            get;
        }
    }
}