using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class BlobPropertyBuilder : ScalarPropertyBuilder
    {
        public BlobPropertyBuilder()
        {
        }

        [DataMember(Order = 1)]
        public int? MaxLength { get; set; }

        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var blobSource = source as BlobPropertyBuilder;
            if (blobSource == null)
            {
                throw new NotSupportedException("The provided source was not a BlobPropertyBuilder");
            }

            this.MaxLength = blobSource.MaxLength;
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var blobSource = source as BlobProperty;
            if (blobSource == null)
            {
                throw new NotSupportedException("The provided source was not a BlobPropertyBuilder");
            }

            this.MaxLength = blobSource.Length;
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new BlobProperty(entity, this, isPrimaryKey);
        }
    }
}