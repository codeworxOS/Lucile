using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.EntityFrameworkCore;
using Lucile.EntityFrameworkCore.Test;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class EntityFrameworkMetadataExtensionsTest
    {
        [Fact]
        public void FromDbIdentityAnPrimaryKeyTest()
        {
            var builder = new MetadataModelBuilder();
            using (var ctx = new TestContext())
            {
                builder.UseDbContext(ctx);
            }

            var model = builder.ToModel();

            var country = model.GetEntityMetadata<Country>();
            var countryIdProp = (ScalarProperty)country["Id"];
            Assert.True(countryIdProp.IsIdentity);
            Assert.True(countryIdProp.IsPrimaryKey);

            var contact = model.GetEntityMetadata<Contact>();
            var contactIdProp = (ScalarProperty)contact["Id"];
            Assert.False(contactIdProp.IsIdentity);
            Assert.True(contactIdProp.IsPrimaryKey);

            var contactSettings = model.GetEntityMetadata<ContactSettings>();
            var contactSettingsIdProperty = (ScalarProperty)contactSettings["Id"];

            Assert.False(contactSettingsIdProperty.IsIdentity);
            Assert.True(contactSettingsIdProperty.IsPrimaryKey);
        }

        [Fact]
        public void FromDbOneToManyMultimplicityTest()
        {
            var builder = new MetadataModelBuilder();
            using (var ctx = new TestContext())
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
            Assert.Equal(1, receiptDetailReceiptProperty.ForeignKeyProperties.Count);
            Assert.Equal(receiptDetail["ReceiptId"], receiptDetailReceiptProperty.ForeignKeyProperties[0].Dependant);
            Assert.Equal(receipt["Id"], receiptDetailReceiptProperty.ForeignKeyProperties[0].Principal);
        }

        [Fact]
        public void FromDbOneToOneMultimplicityTest()
        {
            var builder = new MetadataModelBuilder();
            using (var ctx = new TestContext())
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
    }
}