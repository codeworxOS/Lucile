using System;
using System.Diagnostics;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class MetadataModelBuilderFromModelTest
    {
        [Fact]
        public void CheckHasDefaultValueAfterFromModel_ExpectsOk()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<AllTypesEntity>()
                        .Property(p => p.DateTimeProperty);

            prop.HasDefaultValue = true;

            var model = builder.ToModel();

            builder = new MetadataModelBuilder();
            var newModel = builder.FromModel(model).ToModel();

            var originalProperty = model.GetEntityMetadata<AllTypesEntity>().GetProperties().First(p => p.Name == nameof(AllTypesEntity.DateTimeProperty));
            var newProperty = newModel.GetEntityMetadata<AllTypesEntity>().GetProperties().First(p => p.Name == nameof(AllTypesEntity.DateTimeProperty));

            Assert.True(originalProperty.HasDefaultValue);
            Assert.True(newProperty.HasDefaultValue);

        }
    }
}