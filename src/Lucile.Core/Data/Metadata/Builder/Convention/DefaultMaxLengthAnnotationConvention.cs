using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Lucile.Data.Metadata.Builder.Convention
{
    public class DefaultMaxLengthAnnotationConvention : IEntityConvention
    {
        public void Apply(EntityMetadataBuilder entity)
        {
            foreach (var item in entity.Properties)
            {
                var propertyInfo = entity.TypeInfo.ClrType.GetProperty(item.Name);
                var maxLength = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
                var stringLength = propertyInfo.GetCustomAttribute<StringLengthAttribute>();

                int? length = null;

                if (maxLength != null)
                {
                    length = maxLength.Length;
                }

                if (stringLength != null)
                {
                    length = stringLength.MaximumLength;
                }

                if (length.HasValue)
                {
                    switch (item)
                    {
                        case TextPropertyBuilder textPropertyBuilder:
                            textPropertyBuilder.MaxLength = length;
                            break;
                        case BlobPropertyBuilder blobPropertyBuilder:
                            blobPropertyBuilder.MaxLength = length;
                            break;
                    }
                }
            }
        }
    }
}