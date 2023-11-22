using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class TimeSpanPropertyBuilder : ScalarPropertyBuilder
    {
        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var dateTimeSource = source as TimeSpanPropertyBuilder;
            if (dateTimeSource == null)
            {
                throw new NotSupportedException("The provided source was not a TimeSpanPropertyBuilder");
            }
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var dateTimeSource = source as TimeSpanProperty;
            if (dateTimeSource == null)
            {
                throw new NotSupportedException("The provided source was not a TimeSpanProperty");
            }
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new TimeSpanProperty(entity, this, isPrimaryKey);
        }
    }
}