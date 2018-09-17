﻿using System.IO;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.EntityFrameworkCore;
using Lucile.EntityFrameworkCore.Test;
using Lucile.Test.Model;
using ProtoBuf.Meta;
using Xunit;

namespace Tests
{
#if (NETCOREAPP1_1)

    public class EntityFrameworkCoreMetadataExtensions11Test
#else

    public class EntityFrameworkCoreMetadataExtensions20Test
#endif
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
            Assert.Equal(1, country.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.True(countryIdProp.IsIdentity);
            Assert.True(countryIdProp.IsPrimaryKey);

            var contact = model.GetEntityMetadata<Contact>();
            var contactIdProp = (ScalarProperty)contact["Id"];
            Assert.Equal(1, contact.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.False(contactIdProp.IsIdentity);
            Assert.True(contactIdProp.IsPrimaryKey);

            var contactSettings = model.GetEntityMetadata<ContactSettings>();
            var contactSettingsIdProperty = (ScalarProperty)contactSettings["Id"];
            Assert.Equal(1, contactSettings.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.False(contactSettingsIdProperty.IsIdentity);
            Assert.True(contactSettingsIdProperty.IsPrimaryKey);

            var invoice = model.GetEntityMetadata<Invoice>();
            var invoiceIdProperty = (ScalarProperty)invoice["Id"];
            Assert.Equal(1, invoice.GetProperties().Count(p => p.IsPrimaryKey));
            Assert.False(invoiceIdProperty.IsIdentity);
            Assert.True(invoiceIdProperty.IsPrimaryKey);
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

        [Fact]
        public void FromDbSerializationTest()
        {
            var builder = new MetadataModelBuilder();
            MetadataModelBuilder newBuilder = null;

            using (var ctx = new TestContext())
            {
                builder.UseDbContext(ctx);
            }

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, builder);
                ms.Seek(0, SeekOrigin.Begin);
                newBuilder = ProtoBuf.Serializer.Deserialize<MetadataModelBuilder>(ms);
            }

            foreach (var item in builder.Entities)
            {
                var newItem = newBuilder.Entities.FirstOrDefault(p => p.Name == item.Name);
                Assert.NotNull(newItem);
                foreach (var prop in item.Properties)
                {
                    var newProp = newItem.Properties.FirstOrDefault(p => p.Name == prop.Name);
                    Assert.NotNull(newProp);
                    Assert.Equal(prop.IsExcluded, newProp.IsExcluded);
                    Assert.Equal(prop.IsIdentity, newProp.IsIdentity);
                    Assert.Equal(prop.Nullable, newProp.Nullable);
                }

                foreach (var nav in item.Navigations)
                {
                    var newNav = newItem.Navigations.FirstOrDefault(p => p.Name == nav.Name);
                    Assert.NotNull(newNav);
                    Assert.Equal(nav.IsExcluded, newNav.IsExcluded);
                    Assert.Equal(nav.Multiplicity, newNav.Multiplicity);
                    Assert.Equal(nav.ForeignKey, newNav.ForeignKey);
                    Assert.Equal(nav.Nullable, newNav.Nullable);
                    Assert.Equal(nav.Target, newNav.Target);
                    Assert.Equal(nav.TargetMultiplicity, newNav.TargetMultiplicity);
                    Assert.Equal(nav.TargetProperty, newNav.TargetProperty);
                }
            }
        }
    }
}