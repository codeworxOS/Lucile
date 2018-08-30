using Lucile.Data;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class EntityKeyTest
    {
        [Fact]
        public void EqualityTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>().Property(p => p.ReceiptType);
            builder.Entity<Invoice>().Property(p => p.ReceiptNumber);
            builder.Entity<Invoice>().Property(p => p.Id);
            builder.Entity<Invoice>().Property(p => p.ReceiptDate);
            builder.Entity<Invoice>().PrimaryKey.Add("ReceiptNumber");
            builder.Entity<Invoice>().PrimaryKey.Add("ReceiptType");
            var model = builder.ToModel();

            var entity = model.GetEntityMetadata<Invoice>();
            var invoice = new Invoice { ReceiptNumber = "TestNumber", ReceiptType = ReceiptType.Invoice };

            var key = entity.GetPrimaryKeyObject(invoice);
            var typed = key as EntityKey<string, ReceiptType>;

            Assert.IsType<EntityKey<string, ReceiptType>>(key);
            Assert.Equal(entity, typed.Entity);
            Assert.Equal("TestNumber", typed.Value0);
            Assert.Equal(ReceiptType.Invoice, typed.Value1);

            var keycompare = new EntityKey<string, ReceiptType>(entity)
            {
                Value0 = "TestNumber",
                Value1 = ReceiptType.Invoice
            };

            Assert.Equal(key.GetHashCode(), keycompare.GetHashCode());
            Assert.Equal(key, keycompare);
        }

        [Fact]
        public void InequalityTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>().Property(p => p.ReceiptType);
            builder.Entity<Invoice>().Property(p => p.ReceiptNumber);
            builder.Entity<Invoice>().Property(p => p.Id);
            builder.Entity<Invoice>().Property(p => p.ReceiptDate);
            builder.Entity<Invoice>().PrimaryKey.Add("ReceiptNumber");
            builder.Entity<Invoice>().PrimaryKey.Add("ReceiptType");
            var model = builder.ToModel();

            var entity = model.GetEntityMetadata<Invoice>();
            var invoice = new Invoice { ReceiptNumber = "TestNumber", ReceiptType = ReceiptType.Invoice };

            var key = entity.GetPrimaryKeyObject(invoice);
            var typed = key as EntityKey<string, ReceiptType>;

            var keycompare = new EntityKey<string, ReceiptType>(entity)
            {
                Value0 = "Bla",
                Value1 = ReceiptType.Invoice
            };

            Assert.NotEqual(key.GetHashCode(), keycompare.GetHashCode());
            Assert.NotEqual(key, keycompare);
        }
    }
}