using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class MetadataModelShadowPropertyTest
    {
        [Fact]
        public void TestNoneClrPropertiyInModel_UsePropertyMethod_ExpectsOK()
        {
            var modelBuilder = new MetadataModelBuilder();

            modelBuilder.Entity<Contact>()
                .Property<string>("NoneClrProperty");

            var model = modelBuilder.ToModel();
            var entity = model.GetEntityMetadata<Contact>();

            var property = entity["NoneClrProperty"];

            Assert.NotNull(property);
        }

        [Fact]
        public void TestNoneClrPropertiyInModel_UsePropertyNameProperty_ExpectsOK()
        {
            var modelBuilder = new MetadataModelBuilder();

            var entityBuilder = modelBuilder.Entity<Contact>();
            entityBuilder.Properties.Add(new TextPropertyBuilder { Name = "NoneClrProperty", PropertyType = new ClrTypeInfo(typeof(string)) });

            var model = modelBuilder.ToModel();
            var entity = model.GetEntityMetadata<Contact>();

            var property = entity["NoneClrProperty"];

            Assert.NotNull(property);
        }
    }
}
