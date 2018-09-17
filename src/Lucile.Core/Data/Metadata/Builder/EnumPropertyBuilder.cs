using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class EnumPropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 2)]
        public ClrTypeInfo EnumTypeInfo { get; set; }

        [DataMember(Order = 1)]
        public NumericPropertyType UnderlyingNumericType { get; set; }

        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var enumSource = source as EnumPropertyBuilder;
            if (enumSource == null)
            {
                throw new NotSupportedException("The provided source was not a EnumPropertyBuilder.");
            }

            EnumTypeInfo = enumSource.EnumTypeInfo;
            UnderlyingNumericType = enumSource.UnderlyingNumericType;
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var enumSource = source as EnumProperty;
            if (enumSource == null)
            {
                throw new NotSupportedException("The provided source was not a EnumPropertyBuilder.");
            }

            EnumTypeInfo = new ClrTypeInfo(enumSource.EnumType);
            UnderlyingNumericType = enumSource.UnderlyingNumericType;
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new EnumProperty(entity, this, isPrimaryKey);
        }
    }
}