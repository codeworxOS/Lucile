using System;
using System.Runtime.Serialization;

namespace Lucile.Data.Metadata.Builder
{
    [DataContract(IsReference = true)]
    public class TextPropertyBuilder : ScalarPropertyBuilder
    {
        public TextPropertyBuilder()
        {
        }

        [DataMember(Order = 2)]
        public bool FixedLength { get; set; }

        [DataMember(Order = 1)]
        public int? MaxLength { get; set; }

        [DataMember(Order = 3)]
        public bool Unicode { get; set; }

        protected override void CopyValues(ScalarPropertyBuilder source)
        {
            var textSource = source as TextPropertyBuilder;
            if (textSource == null)
            {
                throw new NotSupportedException("The provided source was not a TextPropertyBuilder.");
            }

            this.MaxLength = textSource.MaxLength;
            this.FixedLength = textSource.FixedLength;
            this.Unicode = textSource.Unicode;
        }

        protected override void CopyValues(ScalarProperty source)
        {
            var textSource = source as TextProperty;
            if (textSource == null)
            {
                throw new NotSupportedException("The provided source was not a TextProperty.");
            }

            this.MaxLength = textSource.MaxLength;
            this.FixedLength = textSource.IsFixedLength;
            this.Unicode = textSource.Unicode;
        }

        protected override ScalarProperty MapToProperty(EntityMetadata entity, bool isPrimaryKey)
        {
            return new TextProperty(entity, this, isPrimaryKey);
        }
    }
}