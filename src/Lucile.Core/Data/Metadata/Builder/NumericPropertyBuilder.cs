using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class NumericPropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 1)]
        public NumericPropertyType NumericType { get; set; }

        [DataMember(Order = 2)]
        public byte Precision { get; set; }

        [DataMember(Order = 3)]
        public byte Scale { get; set; }

        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var numericSource = source as NumericPropertyBuilder;
            if (numericSource == null)
            {
                throw new NotSupportedException("The provided source was not a NumericPropertyBuilder.");
            }

            this.NumericType = numericSource.NumericType;
            this.Precision = numericSource.Precision;
            this.Scale = numericSource.Scale;
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var numericSource = source as NumericProperty;
            if (numericSource == null)
            {
                throw new NotSupportedException("The provided source was not a NumericProperty.");
            }

            this.NumericType = numericSource.NumericPropertyType;
            this.Precision = numericSource.Precision.GetValueOrDefault();
            this.Scale = numericSource.Scale.GetValueOrDefault();
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new NumericProperty(entity, this, isPrimaryKey);
        }
    }
}