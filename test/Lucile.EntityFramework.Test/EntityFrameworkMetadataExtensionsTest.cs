using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.EntityFramework;
using Lucile.EntityFramework.Test;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class EntityFrameworkMetadataExtensionsTest
    {
        [Fact]
        public void DefaultValueAnnotationTest()
        {
            var builder = new MetadataModelBuilder();

            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();

            var prop = model.GetEntityMetadata<ArticleName>().GetProperties().First(p => p.Name == "TranlatedText");
            var prop2 = model.GetEntityMetadata<ArticleName>().GetProperties().First(p => p.Name == "LanguageId");

            Assert.True(prop.HasDefaultValue);
            Assert.False(prop2.HasDefaultValue);
        }

        [Fact]
        public void MaxLengthAnnotation_ExpectsValue()
        {
            var builder = new MetadataModelBuilder();

            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();
            var entity = model.GetEntityMetadata<ArticleName>();

            Assert.Equal(255, ((TextProperty)entity.GetProperties().First(p => p.Name == nameof(ArticleName.TranlatedText))).MaxLength);
        }

        [Fact]
        public void FromDbIdentityAnPrimaryKeyTest()
        {
            var builder = new MetadataModelBuilder();

            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();

            var country = model.GetEntityMetadata<Country>();
            var countryIdProp = (ScalarProperty)country["Id"];
            Assert.Equal(1, country.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.Equal(AutoGenerateValue.OnInsert, countryIdProp.ValueGeneration);
            Assert.True(countryIdProp.IsPrimaryKey);

            var contact = model.GetEntityMetadata<Contact>();
            var contactIdProp = (ScalarProperty)contact["Id"];
            Assert.Equal(1, contact.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.Equal(AutoGenerateValue.None, contactIdProp.ValueGeneration);
            Assert.True(contactIdProp.IsPrimaryKey);

            var contactSettings = model.GetEntityMetadata<ContactSettings>();
            var contactSettingsIdProperty = (ScalarProperty)contactSettings["Id"];
            Assert.Equal(1, contactSettings.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.Equal(AutoGenerateValue.None, contactSettingsIdProperty.ValueGeneration);
            Assert.True(contactSettingsIdProperty.IsPrimaryKey);

            var invoice = model.GetEntityMetadata<Invoice>();
            var invoiceIdProperty = (ScalarProperty)invoice["Id"];
            Assert.Equal(1, invoice.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.Equal(AutoGenerateValue.None, invoiceIdProperty.ValueGeneration);
            Assert.True(invoiceIdProperty.IsPrimaryKey);
        }

        [Fact]
        public void FromDbOneToManyMultimplicityTest()
        {
            var builder = new MetadataModelBuilder();
            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();
            var receipt = model.GetEntityMetadata<Receipt>();
            var receiptDetail = model.GetEntityMetadata<ReceiptDetail>();

            var receiptDetailsProperty = (NavigationPropertyMetadata)receipt["Details"];
            var receiptDetailReceiptProperty = (NavigationPropertyMetadata)receiptDetail["Receipt"];

            Assert.Equal(NavigationPropertyMultiplicity.Many, receiptDetailsProperty.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, receiptDetailsProperty.TargetMultiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, receiptDetailReceiptProperty.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, receiptDetailReceiptProperty.TargetMultiplicity);

            Assert.Equal(receiptDetailsProperty.TargetNavigationProperty, receiptDetailReceiptProperty);
            Assert.Equal(receiptDetailReceiptProperty.TargetNavigationProperty, receiptDetailsProperty);
            Assert.Single(receiptDetailReceiptProperty.ForeignKeyProperties);
            Assert.Equal(receiptDetail["ReceiptId"], receiptDetailReceiptProperty.ForeignKeyProperties[0].Dependant);
            Assert.Equal(receipt["Id"], receiptDetailReceiptProperty.ForeignKeyProperties[0].Principal);
        }

        [Fact]
        public void FromDbOneToOneMultimplicityTest()
        {
            var builder = new MetadataModelBuilder();
            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();
            var contact = model.GetEntityMetadata<Contact>();

            var contactSettings = model.GetEntityMetadata<ContactSettings>();
            var contactSettingsContactProperty = (NavigationPropertyMetadata)contactSettings["Contact"];
            Assert.Equal(NavigationPropertyMultiplicity.One, contactSettingsContactProperty.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, contactSettingsContactProperty.TargetMultiplicity);
            Assert.Null(contactSettingsContactProperty.TargetNavigationProperty);
            Assert.Equal(contact, contactSettingsContactProperty.TargetEntity);
        }

        [Fact]
        public void MetadataDataContractSerializationTest()
        {
            var builder = new MetadataModelBuilder();
            MetadataModelBuilder clone = null;
            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();

            var dc = new DataContractSerializer(typeof(MetadataModelBuilder));
            using (var ms = new MemoryStream())
            {
                dc.WriteObject(ms, builder);
                ms.Seek(0, SeekOrigin.Begin);
                clone = dc.ReadObject(ms) as MetadataModelBuilder;
            }

            var cloneModel = clone.ToModel();

            Assert.Equal(model.Entities, cloneModel.Entities, new EntityMetadataComparer());
        }

        [Fact]
        public void MetadataProtobufSerializationTest()
        {
            var builder = new MetadataModelBuilder();
            MetadataModelBuilder clone = null;
            using (var ctx = CreateContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();

            var proto = ProtoBuf.Meta.RuntimeTypeModel.Default.CreateFormatter(typeof(MetadataModelBuilder));
            using (var ms = new MemoryStream())
            {
                proto.Serialize(ms, builder);
                ms.Seek(0, SeekOrigin.Begin);
                clone = proto.Deserialize(ms) as MetadataModelBuilder;
            }

            var cloneModel = clone.ToModel();

            Assert.Equal(model.Entities, cloneModel.Entities, new EntityMetadataComparer());
        }

        private static DbContext CreateContext()
        {
            var info = new DbContextInfo(typeof(TestContext), new DbProviderInfo("System.Data.SqlClient", "2012"));

            var ctx = info.CreateInstance();
            ctx.Database.Connection.ConnectionString = "Data Source=.\notneeded;Initial Catalog=EfUnitTest;Integrated Security=True";

            return ctx;
        }
    }
}