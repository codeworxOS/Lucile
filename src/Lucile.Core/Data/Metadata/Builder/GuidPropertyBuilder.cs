using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GuidPropertyBuilder : ScalarPropertyBuilder
    {
        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var guidSource = source as GuidPropertyBuilder;
            if (guidSource == null)
            {
                throw new NotSupportedException("The provided source was not a GuidPropertyBuilder.");
            }
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var guidSource = source as GuidProperty;
            if (guidSource == null)
            {
                throw new NotSupportedException("The provided source was not a GuidProperty.");
            }
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GuidProperty(entity, this, isPrimaryKey);
        }
    }
}