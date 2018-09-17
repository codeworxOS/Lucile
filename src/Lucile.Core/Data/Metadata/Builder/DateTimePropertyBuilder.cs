using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class DateTimePropertyBuilder : ScalarPropertyBuilder
    {
        [DataMember(Order = 1)]
        public DateTimePropertyType DateTimeType { get; set; }

        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var dateTimeSource = source as DateTimePropertyBuilder;
            if (dateTimeSource == null)
            {
                throw new NotSupportedException("The provided source was not a DateTimePropertyBuilder");
            }

            this.DateTimeType = dateTimeSource.DateTimeType;
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var dateTimeSource = source as DateTimeProperty;
            if (dateTimeSource == null)
            {
                throw new NotSupportedException("The provided source was not a DateTimePropertyBuilder");
            }

            this.DateTimeType = dateTimeSource.DateTimePropertyType;
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new DateTimeProperty(entity, this, isPrimaryKey);
        }
    }
}