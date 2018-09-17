using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class GeometryPropertyBuilder : ScalarPropertyBuilder
    {
        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var geometrySource = source as GeometryPropertyBuilder;
            if (geometrySource == null)
            {
                throw new NotSupportedException("The provided source was not a GeometryPropertyBuilder.");
            }
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var geometrySource = source as GeometryProperty;
            if (geometrySource == null)
            {
                throw new NotSupportedException("The provided source was not a GeometryPropertyBuilder.");
            }
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new GeometryProperty(entity, this, isPrimaryKey);
        }
    }
}