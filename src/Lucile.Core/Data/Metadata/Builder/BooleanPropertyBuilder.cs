using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class BooleanPropertyBuilder : ScalarPropertyBuilder
    {
        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var boolSource = source as BooleanPropertyBuilder;
            if (boolSource == null)
            {
                throw new NotSupportedException("The provided source was not a BooleanPropertyBuilder");
            }
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new BooleanProperty(entity, this, isPrimaryKey);
        }
    }
}