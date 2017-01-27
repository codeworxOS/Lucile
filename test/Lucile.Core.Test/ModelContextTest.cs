using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucile.Data;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Data.Tracking;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class ModelContextTest
    {
        [Fact]
        public void AutomaticFixupTest()
        {
            var metadataModelBuilder = new MetadataModelBuilder();
            metadataModelBuilder.Entity<ArticleName>().PrimaryKey.AddRange(new[] { "ArticleId", "LanguageId" });
            var articleEntity = metadataModelBuilder.Entity<Article>();
            metadataModelBuilder.Entity<ReceiptDetail>();
            metadataModelBuilder.Entity<Invoice>().BaseEntity = metadataModelBuilder.Entity(typeof(Receipt));
            var contactSettings = metadataModelBuilder.Entity<ContactSettings>();

            articleEntity.HasOne(p => p.ArticleSettings).WithPrincipal();
            contactSettings.HasOne(p => p.Contact).WithDependant();

            metadataModelBuilder.ApplyConventions();

            var invoice = new Invoice { Id = Guid.NewGuid() };
            var article = new Article { Id = Guid.NewGuid(), ArticleNumber = "12314", SupplierId = Guid.NewGuid(), ArticleDescription = "testchen" };

            var detail = new ReceiptDetail { Id = Guid.NewGuid(), Receipt = invoice, ReceiptId = invoice.Id, Amount = 123, ArticleId = article.Id };
            invoice.Details.Add(detail);

            var name1 = new ArticleName { ArticleId = article.Id, LanguageId = "de", TranlatedText = "Text 1" };
            var name2 = new ArticleName { ArticleId = article.Id, LanguageId = "en", TranlatedText = "Text 1" };

            var context = new ModelContext(metadataModelBuilder.ToModel());
            context.Attach(new[] { name1, name2 });
            context.AttachSingle(article);

            Assert.Equal(article, name1.Article);
            Assert.Equal(article, name2.Article);
            Assert.Equal(2, article.Names.Count);
            Assert.Contains(article.Names, p => p == name1);
            Assert.Contains(article.Names, p => p == name2);

            context.AttachSingle(invoice);

            Assert.Equal(article, detail.Article);

            Assert.Null(article.ArticleSettings);

            var articleSettings = new ArticleSettings { Id = article.Id, Whatever = "test" };
            context.AttachSingle(articleSettings);

            Assert.Equal(article.ArticleSettings, articleSettings);
        }

        [Fact]
        public void GetChangesTest()
        {
            var metadataModelBuilder = new MetadataModelBuilder();
            metadataModelBuilder.Exclude<EntityBase>();
            metadataModelBuilder.Entity<ArticleName>().PrimaryKey.AddRange(new[] { "ArticleId", "LanguageId" });
            var articleEntity = metadataModelBuilder.Entity<Article>();
            metadataModelBuilder.Entity<ReceiptDetail>();
            metadataModelBuilder.Entity<Invoice>().BaseEntity = metadataModelBuilder.Entity(typeof(Receipt));
            var contactSettings = metadataModelBuilder.Entity<ContactSettings>();

            articleEntity.HasOne(p => p.ArticleSettings).WithPrincipal();
            contactSettings.HasOne(p => p.Contact).WithDependant();

            metadataModelBuilder.ApplyConventions();

            var model = metadataModelBuilder.ToModel();

            var country = new Country { Id = 1, CountryName = "Test" };
            var customer = new Contact { Id = Guid.NewGuid(), FirstName = "Max", LastName = "Mustermann", ContactType = ContactType.Customer, Country = country, CountryId = country.Id };
            var customer2 = new Contact { Id = Guid.NewGuid(), FirstName = "Max", LastName = "Mustermann", ContactType = ContactType.Customer, Country = country, CountryId = country.Id };

            var supplier = new Contact { Id = Guid.NewGuid(), FirstName = "Max", LastName = "Supplier", ContactType = ContactType.Supplier, Country = country, CountryId = country.Id };
            var receipt = new Invoice { Id = Guid.NewGuid(), Customer = customer, CustomerId = customer.Id, ReceiptNumber = "12345", ReceiptDate = DateTime.Now, LastPaymentDate = DateTime.Now, ReceiptType = ReceiptType.Invoice };
            var article = new Article { Id = Guid.NewGuid(), ArticleDescription = "Description", Supplier = supplier, SupplierId = supplier.Id, ArticleNumber = "12314" };
            var article2 = new Article { Id = Guid.NewGuid(), ArticleDescription = "Description", Supplier = supplier, SupplierId = supplier.Id, ArticleNumber = "12314" };
            var detail = new ReceiptDetail { Id = Guid.NewGuid(), Amount = 123, Description = "bla", Receipt = receipt, ReceiptId = receipt.Id, Article = article, ArticleId = article.Id };
            receipt.Details.Add(detail);

            var context = new ModelContext(model);
            context.AttachSingle(receipt);

            context.TrackedObjects.OfType<ITrackable>().ToList().ForEach(p => p.State = TrackingState.Unchanged);

            customer2.State = TrackingState.Added;
            article2.State = TrackingState.Added;

            context.AttachSingle(customer2);
            context.AttachSingle(article2);

            detail.ArticleId = article2.Id;
            detail.Article = article2;
            detail.State = TrackingState.Modified;

            receipt.Customer = customer2;
            receipt.CustomerId = customer2.Id;
            receipt.State = TrackingState.Modified;

            customer.State = TrackingState.Deleted;

            var changes = context.GetChanges().OfType<ITrackable>().ToList();

            Assert.Equal(5, changes.Count);
            Assert.IsType<Contact>(changes[0]);
            Assert.Equal(TrackingState.Added, changes[0].State);

            Assert.IsType<Article>(changes[1]);
            Assert.Equal(TrackingState.Added, changes[1].State);

            Assert.IsType<Invoice>(changes[2]);
            Assert.Equal(TrackingState.Modified, changes[2].State);

            Assert.IsType<ReceiptDetail>(changes[3]);
            Assert.Equal(TrackingState.Modified, changes[3].State);

            Assert.IsType<Contact>(changes[4]);
            Assert.Equal(TrackingState.Deleted, changes[4].State);
        }
    }
}