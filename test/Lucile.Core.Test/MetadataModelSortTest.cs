using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class MetadataModelSortTest
    {
        [Fact]
        public void TestMetadataModelSortWithLatvianCultureInfo()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("lv-LV");

            var builder = new MetadataModelBuilder();
            var entity = builder.Entity<YsortTest>();
            entity.Property(p => p.Id);
            entity.Property(p => p.JFirstProperty);
            entity.Property(p => p.YSecondProperty);

            var entity2 = builder.Entity<Receipt>();
            entity2.Property(p => p.Id);
            entity2.Property(p => p.ReceiptNumber);

            var model = builder.ToModel();

            var yEntity = model.GetEntityMetadata<YsortTest>();
            var receiptEntity = model.GetEntityMetadata<Receipt>();

            var receiptIndex = model.Entities.IndexOf(receiptEntity);
            var yEntityIndex = model.Entities.IndexOf(yEntity);

            Assert.True(yEntityIndex > receiptIndex);

            var firstProp = yEntity[nameof(YsortTest.JFirstProperty)];
            var secondProp = yEntity[nameof(YsortTest.YSecondProperty)];

            var yproperties = yEntity.GetProperties().OfType<PropertyMetadata>().ToList();

            Assert.True(yproperties.IndexOf(firstProp) < yproperties.IndexOf(secondProp));

        }
    }
}