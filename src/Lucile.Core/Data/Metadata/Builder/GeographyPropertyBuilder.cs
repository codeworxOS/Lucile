using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GeographyPropertyBuilder : ScalarPropertyBuilder
    {
        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var geographySource = source as GeographyPropertyBuilder;
            if (geographySource == null)
            {
                throw new NotSupportedException("The provided source was not a GeographyPropertyBuilder.");
            }
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var geographySource = source as GeographyProperty;
            if (geographySource == null)
            {
                throw new NotSupportedException("The provided source was not a GeographyPropertyBuilder.");
            }
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GeographyProperty(entity, this, isPrimaryKey);
        }
    }
}