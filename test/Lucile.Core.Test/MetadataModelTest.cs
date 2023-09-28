using System;
using System.Diagnostics;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class MetadataModelTest
    {
        [Fact]
        public void EntityMetadataBuilderBlobProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<AllTypesEntity>()
                        .Property(p => p.BlobProperty);

            Assert.IsType<BlobPropertyBuilder>(prop);
            Assert.Equal("BlobProperty", prop.Name);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderEnumProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<Receipt>()
                        .Property(p => p.ReceiptType);

            Assert.Equal("ReceiptType", prop.Name);
            Assert.Equal(NumericPropertyType.Byte, prop.UnderlyingNumericType);
            Assert.Equal(typeof(ReceiptType), prop.EnumTypeInfo.ClrType);
            Assert.False(prop.Nullable);

            prop = builder.Entity<Contact>()
                .Property(p => p.ContactType);

            Assert.Equal("ContactType", prop.Name);
            Assert.Equal(NumericPropertyType.Int32, prop.UnderlyingNumericType);
            Assert.Equal(typeof(ContactType), prop.EnumTypeInfo.ClrType);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderGenericBlobProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<AllTypesEntity>()
                        .Property(p => p.BlobProperty);

            Assert.IsType<BlobPropertyBuilder>(prop);
            Assert.Equal("BlobProperty", prop.Name);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderGenericEnumProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<Receipt>()
                        .Property(p => p.ReceiptType);

            Assert.IsType<EnumPropertyBuilder>(prop);
            Assert.Equal("ReceiptType", prop.Name);
            Assert.Equal(NumericPropertyType.Byte, prop.UnderlyingNumericType);
            Assert.Equal(typeof(ReceiptType), prop.EnumTypeInfo.ClrType);
            Assert.False(prop.Nullable);

            prop = builder.Entity<Contact>()
            .Property(p => p.ContactType);

            Assert.IsType<EnumPropertyBuilder>(prop);
            Assert.Equal("ContactType", prop.Name);
            Assert.Equal(NumericPropertyType.Int32, prop.UnderlyingNumericType);
            Assert.Equal(typeof(ContactType), prop.EnumTypeInfo.ClrType);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderGenericNumericProperty()
        {
            var builder = new MetadataModelBuilder().Entity<AllTypesEntity>();

            var prop = builder.Property(p => p.NullableByteProperty);
            Assert.Equal(NumericPropertyType.Byte, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ByteProperty);
            Assert.Equal(NumericPropertyType.Byte, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableSByteProperty);
            Assert.Equal(NumericPropertyType.SByte, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.SByteProperty);
            Assert.Equal(NumericPropertyType.SByte, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableShortProperty);
            Assert.Equal(NumericPropertyType.Int16, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ShortProperty);
            Assert.Equal(NumericPropertyType.Int16, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableUShortProperty);
            Assert.Equal(NumericPropertyType.UInt16, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.UShortProperty);
            Assert.Equal(NumericPropertyType.UInt16, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableIntProperty);
            Assert.Equal(NumericPropertyType.Int32, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.IntProperty);
            Assert.Equal(NumericPropertyType.Int32, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableUIntProperty);
            Assert.Equal(NumericPropertyType.UInt32, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.UIntProperty);
            Assert.Equal(NumericPropertyType.UInt32, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableLongProperty);
            Assert.Equal(NumericPropertyType.Int64, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.LongProperty);
            Assert.Equal(NumericPropertyType.Int64, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableULongProperty);
            Assert.Equal(NumericPropertyType.UInt64, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ULongProperty);
            Assert.Equal(NumericPropertyType.UInt64, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableFloatProperty);
            Assert.Equal(NumericPropertyType.Single, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.FloatProperty);
            Assert.Equal(NumericPropertyType.Single, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableDoubleProperty);
            Assert.Equal(NumericPropertyType.Double, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.DoubleProperty);
            Assert.Equal(NumericPropertyType.Double, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableDecimalProperty);
            Assert.Equal(NumericPropertyType.Decimal, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.DecimalProperty);
            Assert.Equal(NumericPropertyType.Decimal, prop.NumericType);
            Assert.False(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderTimeSpanProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<AllTypesEntity>()
                        .Property(p => p.TimeSpanProperty);

            Assert.IsType<TimeSpanPropertyBuilder>(prop);
            Assert.Equal("TimeSpanProperty", prop.Name);
            Assert.False(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderNullableTimeSpanProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<AllTypesEntity>()
                        .Property(p => p.NullableTimeSpanProperty);

            Assert.IsType<TimeSpanPropertyBuilder>(prop);
            Assert.Equal("NullableTimeSpanProperty", prop.Name);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderGenericTextProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<Receipt>()
                        .Property(p => p.ReceiptNumber);

            Assert.IsType<TextPropertyBuilder>(prop);
            Assert.Equal("ReceiptNumber", prop.Name);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderNumericProperty()
        {
            var builder = new MetadataModelBuilder().Entity<AllTypesEntity>();

            var prop = builder.Property(p => p.NullableByteProperty);
            Assert.Equal(NumericPropertyType.Byte, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ByteProperty);
            Assert.Equal(NumericPropertyType.Byte, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableSByteProperty);
            Assert.Equal(NumericPropertyType.SByte, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.SByteProperty);
            Assert.Equal(NumericPropertyType.SByte, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableShortProperty);
            Assert.Equal(NumericPropertyType.Int16, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ShortProperty);
            Assert.Equal(NumericPropertyType.Int16, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableUShortProperty);
            Assert.Equal(NumericPropertyType.UInt16, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.UShortProperty);
            Assert.Equal(NumericPropertyType.UInt16, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableIntProperty);
            Assert.Equal(NumericPropertyType.Int32, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.IntProperty);
            Assert.Equal(NumericPropertyType.Int32, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableUIntProperty);
            Assert.Equal(NumericPropertyType.UInt32, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.UIntProperty);
            Assert.Equal(NumericPropertyType.UInt32, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableLongProperty);
            Assert.Equal(NumericPropertyType.Int64, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.LongProperty);
            Assert.Equal(NumericPropertyType.Int64, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableULongProperty);
            Assert.Equal(NumericPropertyType.UInt64, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.ULongProperty);
            Assert.Equal(NumericPropertyType.UInt64, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableFloatProperty);
            Assert.Equal(NumericPropertyType.Single, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.FloatProperty);
            Assert.Equal(NumericPropertyType.Single, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableDoubleProperty);
            Assert.Equal(NumericPropertyType.Double, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.DoubleProperty);
            Assert.Equal(NumericPropertyType.Double, prop.NumericType);
            Assert.False(prop.Nullable);

            prop = builder.Property(p => p.NullableDecimalProperty);
            Assert.Equal(NumericPropertyType.Decimal, prop.NumericType);
            Assert.True(prop.Nullable);

            prop = builder.Property(p => p.DecimalProperty);
            Assert.Equal(NumericPropertyType.Decimal, prop.NumericType);
            Assert.False(prop.Nullable);
        }

        [Fact]
        public void EntityMetadataBuilderReplacetItemWithNullTest()
        {
            var builder = new MetadataModelBuilder();

            var receiptBuilder = builder.Entity<Receipt>();

            receiptBuilder.HasMany(p => p.Details)
                .WithOne(p => p.Receipt)
                .HasForeignKey("ReceiptId");
            receiptBuilder.Property(p => p.Id).Nullable = false;
            receiptBuilder.PrimaryKey.Add("Id");

            builder.Entity<Invoice>().BaseEntity = receiptBuilder;

            var receiptDetailBuilder = builder.Entity<ReceiptDetail>();
            receiptDetailBuilder.PrimaryKey.Add("Id");
            receiptDetailBuilder.Property(p => p.Id).Nullable = false;
            receiptDetailBuilder.Property(p => p.ReceiptId).Nullable = false;

            var model = builder.ToModel();

            var receipt = new Invoice { Id = Guid.NewGuid() };
            var rd1 = new ReceiptDetail { Id = Guid.NewGuid(), ReceiptId = receipt.Id };
            var rd2 = new ReceiptDetail { Id = Guid.NewGuid(), ReceiptId = receipt.Id };

            receipt.Details.AddRange(new[] { rd1, rd2 });

            var entity = model.GetEntityMetadata(receipt);
            var details = entity.GetNavigations().First(p => p.Name == "Details");

            Assert.Equal(2, receipt.Details.Count);
            Assert.All(receipt.Details, p => Assert.NotNull(p));

            details.ReplaceItem(receipt, rd1, null);

            Assert.Single(receipt.Details);
            Assert.All(receipt.Details, p => Assert.NotNull(p));
        }

        [Fact]
        public void EntityMetadataBuilderTextProperty()
        {
            var builder = new MetadataModelBuilder();
            var prop = builder.Entity<Receipt>()
                        .Property("ReceiptNumber", typeof(string));

            Assert.IsType<TextPropertyBuilder>(prop);
            Assert.Equal("ReceiptNumber", prop.Name);
            Assert.True(prop.Nullable);
        }

        [Fact]
        public void GetEntityInheritenceTest()
        {
            var builder = new MetadataModelBuilder();
            var receipt = builder.Entity<Receipt>();
            var invoice = builder.Entity<Invoice>();
            invoice.BaseEntity = receipt;
            var order = builder.Entity<Order>();
            order.BaseEntity = invoice;

            var model = builder.ToModel();

            var invoiceInstance = new Invoice();
            var entity = model.GetEntityMetadata(invoiceInstance);
            Assert.Equal("Invoice", entity.Name);
            entity = model.GetEntityMetadata<Order>();

            Assert.Equal("Order", entity.Name);
        }

        [Fact]
        public void GetMetadataPerformanceTest()
        {
            var builder = new MetadataModelBuilder();
            var receipt = builder.Entity<Receipt>();
            var invoice = builder.Entity<Invoice>();
            var order = builder.Entity<Order>();

            invoice.Property(p => p.Id);
            invoice.PrimaryKey.Add("Id");

            var model = builder.ToModel();

            var inv = new Invoice { Id = Guid.NewGuid() };
            var ord = new Order { Id = Guid.NewGuid() };

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 2000000; i++)
            {
                if (i % 2 == 0)
                {
                    var entity = model.GetEntityMetadata(inv);
                }
                else
                {
                    var entity = model.GetEntityMetadata(ord);
                }
            }

            sw.Stop();

            Assert.True(sw.Elapsed.TotalSeconds < 1, "too slow");
        }

        [Fact]
        public void TestNoneClrForeignKeyProperty_Expects_Ok()
        {
            var builder = new MetadataModelBuilder();

            var article = builder.Entity<Article>();
            article.PrimaryKey.Add("Id");
            article.Property(p => p.Id);
            article.Property("CountryId", typeof(Guid?));

            var nav = new NavigationPropertyBuilder();
            nav.Name = "Country";
            nav.ForeignKey.Add("CountryId");
            nav.Target = new ClrTypeInfo(typeof(Country));
            nav.Nullable = true;
            article.Navigations.Add(nav);

            var country = builder.Entity<Country>();
            country.Property(p => p.Id);
            country.PrimaryKey.Add("Id");

            var model = builder.ToModel();
        }

        [Fact]
        public void TestNoneExistingEntityType_Expects_Ok()
        {
            var builder = new MetadataModelBuilder();

            var article = builder.Entity<Article>();
            article.PrimaryKey.Add("Id");
            article.Property(p => p.Id);

            var country = builder.Entity<Country>();
            country.Property(p => p.Id);
            country.PrimaryKey.Add("Id");

            var current = builder.Entities.ToList();

            var entityBuilder = new EntityMetadataBuilder()
            {
                TypeInfo = new ClrTypeInfo { ClrTypeName = "Definitily.None.Existing.SampleEntity, Definitily.None.Existing" },
            };

            entityBuilder.Properties.Add(new GuidPropertyBuilder { Name = "Id", PropertyType = new ClrTypeInfo(typeof(Guid)) });
            entityBuilder.PrimaryKey.Add("Id");

            current.Add(entityBuilder);

            builder.Entities = current;

            var model = builder.ToModel();

            Assert.Contains(model.Entities, p => p.Name == "SampleEntity");
            var entity = model.Entities.Find(p => p.Name == "SampleEntity");
            Assert.Null(entity.ClrType);

        }

        [Fact]
        public void GetTargetNavigationPropertyPerformance()
        {
            var builder = new MetadataModelBuilder();

            var receiptBuilder = builder.Entity<Receipt>();

            receiptBuilder.HasMany(p => p.Details)
                    .WithOne(p => p.Receipt)
                    .HasForeignKey("ReceiptId");
            receiptBuilder.Property(p => p.Id).Nullable = false;
            receiptBuilder.PrimaryKey.Add("Id");

            builder.Entity<Invoice>().BaseEntity = receiptBuilder;

            var receiptDetailBuilder = builder.Entity<ReceiptDetail>();
            receiptDetailBuilder.PrimaryKey.Add("Id");
            receiptDetailBuilder.Property(p => p.Id).Nullable = false;
            receiptDetailBuilder.Property(p => p.ReceiptId).Nullable = false;

            var model = builder.ToModel();

            var receipt = model.GetEntityMetadata<Receipt>();
            var details = receipt.GetNavigations().First(p => p.Name == "Details");

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 2000000; i++)
            {
                var targetProp = details.TargetNavigationProperty;
            }
            sw.Stop();

            Assert.True(sw.Elapsed.TotalSeconds < 1);
        }

        [Fact]
        public void MetadataModelBuilderEntityTest()
        {
            var builder = new MetadataModelBuilder();
            var entityBuilder = builder.Entity(typeof(Receipt));

            Assert.Equal("Receipt", entityBuilder.Name);
            Assert.Equal(typeof(Receipt), entityBuilder.TypeInfo.ClrType);
        }

        [Fact]
        public void MetadataModelBuilderFromModelTest()
        {
            var builder = new MetadataModelBuilder();
            var entityBuilder = builder.Entity<Receipt>();
            entityBuilder.Property(p => p.Id).ValueGeneration = AutoGenerateValue.OnInsert;
            var nr = entityBuilder.Property(p => p.ReceiptNumber);
            nr.MaxLength = 20;
            nr.Nullable = false;

            entityBuilder.PrimaryKey.Add("Id");

            var invoice = builder.Entity<Invoice>();
            invoice.BaseEntity = entityBuilder;
            var delivery = invoice.Property(p => p.ExpectedDeliveryDate);
            delivery.DateTimeType = DateTimePropertyType.DateTime;

            var oldModel = builder.ToModel();

            var newBuilder = new MetadataModelBuilder().FromModel(oldModel);
            var newModel = newBuilder.ToModel();

            Assert.All(oldModel.Entities, p => Assert.NotNull(newModel.GetEntityMetadata(p.ClrType)));
            Assert.All(oldModel.Entities,
                p =>
                {
                    var newEntity = newModel.GetEntityMetadata(p.ClrType);
                    Assert.All(p.GetProperties(), x => Assert.NotNull(newEntity.GetProperties().FirstOrDefault(y => y.Name == x.Name)));
                });
        }

        [Fact]
        public void MetadataModelBuilderGenericEntityTest()
        {
            var builder = new MetadataModelBuilder();
            var entityBuilder = builder.Entity<Receipt>();

            Assert.Equal("Receipt", entityBuilder.Name);
            Assert.Equal(typeof(Receipt), entityBuilder.TypeInfo.ClrType);
        }

        [Fact]
        public void MetadataModelBuilderMaxLengthTest()
        {
            var builder = new MetadataModelBuilder();
            var entityBuilder = builder.Entity<Receipt>();
            builder.ApplyConventions();

            Assert.Equal(100, entityBuilder.Property(c => c.ReceiptNumber).MaxLength);
        }


        [Fact]
        public void MetadataModelSortByDependencyTest()
        {
            var metadataModelBuilder = new MetadataModelBuilder();
            var articleEntity = metadataModelBuilder.Entity<Article>();
            var receiptEntity = metadataModelBuilder.Entity<Receipt>();
            var receiptDetailEntity = metadataModelBuilder.Entity<ReceiptDetail>();
            var articleSettingsEntity = metadataModelBuilder.Entity<ArticleSettings>();

            articleSettingsEntity.Property(p => p.Id);
            articleSettingsEntity.Property(p => p.Whatever);
            articleSettingsEntity.PrimaryKey.Add("Id");

            articleEntity.Property(p => p.Id);
            articleEntity.Property(p => p.Price);
            articleEntity.PrimaryKey.Add("Id");

            receiptEntity.Property(p => p.Id);
            receiptEntity.Property(p => p.ReceiptNumber);
            receiptEntity.PrimaryKey.Add("Id");

            receiptDetailEntity.Property(p => p.Id);
            receiptDetailEntity.Property(p => p.ReceiptId);
            receiptDetailEntity.Property(p => p.ArticleId);
            receiptDetailEntity.PrimaryKey.Add("Id");

            articleEntity.HasOne(p => p.ArticleSettings).WithPrincipal();
            receiptDetailEntity.HasOne(p => p.Receipt).WithMany(p => p.Details).HasForeignKey("ReceiptId");
            receiptDetailEntity.HasOne(p => p.Article).WithMany().HasForeignKey("ArticleId");

            var model = metadataModelBuilder.ToModel();

            var sorted = model.SortedByDependency();

            Assert.Contains(sorted.Take(2), p => p.Name == "Article");
            Assert.Contains(sorted.Take(2), p => p.Name == "Receipt");
            Assert.Contains(sorted.Skip(2), p => p.Name == "ArticleSettings");
            Assert.Contains(sorted.Skip(2), p => p.Name == "ReceiptDetail");
        }
    }
}