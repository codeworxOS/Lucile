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
        public void AttachOperationsMergeTest()
        {
            MetadataModel model = GetModel();

            var context = new ModelContext(model);

            var customerOld = new Contact { State = TrackingState.Unchanged, Id = Guid.NewGuid(), FirstName = "First1", LastName = "Last1" };
            var customerNew = new Contact { State = TrackingState.Unchanged, Id = customerOld.Id, FirstName = "First1", LastName = "Last2" };
            var customerOther = new Contact { State = TrackingState.Unchanged, Id = Guid.NewGuid(), FirstName = "FirstOther", LastName = "LastOther" };

            context.AttachSingle(customerOld);

            var operations = context.Merge(new[] { customerNew, customerOther });

            Assert.Equal(2, operations.Items.Count());
            Assert.Equal(customerOld, operations.Items.First());
            Assert.Single(operations.Added);
            Assert.Equal(customerOther, operations.Added.First());
            Assert.Single(operations.Merged);
            Assert.Equal(customerOld, operations.Merged.First().Key);
            Assert.Single(operations.Merged.First().Value);
            Assert.Equal(nameof(Contact.LastName), operations.Merged.First().Value.First().Name);
        }

        [Fact]
        public void AttachWithNullEntryInNavigationProperty()
        {
            var model = GetModel();
            var ctx = new ModelContext(model);

            var receipt = new Invoice { Id = Guid.NewGuid() };
            var rd1 = new ReceiptDetail { Id = Guid.NewGuid(), ReceiptId = receipt.Id };
            var rd2 = new ReceiptDetail { Id = Guid.NewGuid(), ReceiptId = receipt.Id };

            receipt.Details.AddRange(new[] { rd1, null, rd2 });

            receipt = ctx.AttachSingle(receipt);
        }

        [Fact]
        public void AutomaticFixupTest()
        {
            var metadataModelBuilder = new MetadataModelBuilder();
            var articleNameEntity = metadataModelBuilder.Entity<ArticleName>();
            articleNameEntity.PrimaryKey.AddRange(new[] { "ArticleId", "LanguageId" });
            var articleEntity = metadataModelBuilder.Entity<Article>();
            metadataModelBuilder.Entity<ReceiptDetail>();
            metadataModelBuilder.Entity<Invoice>().BaseEntity = metadataModelBuilder.Entity(typeof(Receipt));
            var contactSettings = metadataModelBuilder.Entity<ContactSettings>();

            articleNameEntity.HasMany(p => p.History).WithOne(p => p.ArticleName).HasForeignKey("ArticleId", "LanguageId");

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
        public void AttachObjectWithCompositeKeyTest()
        {
            var model = GetModel();
            var ctx = new ModelContext(model);

            var article = new Article { Id = Guid.NewGuid(), ArticleNumber = "12314", SupplierId = Guid.NewGuid(), ArticleDescription = "testchen" };
            var articleName = new ArticleName { ArticleId = article.Id, LanguageId = "de", TranlatedText = "first" };


            ctx.AttachSingle(articleName);
            ctx.AttachSingle(article);

            Assert.NotEmpty(article.Names);
            Assert.Equal(article.Names.First(), articleName);

            var articleNameHistory = new ArticleNameHistory { Id = Guid.NewGuid(), ArticleId = article.Id, LanguageId = "de", ChangeDate = DateTime.Now };

            ctx.AttachSingle(articleNameHistory);

            Assert.NotNull(articleNameHistory.ArticleName);
            Assert.Equal(articleName.History.First(), articleNameHistory);

            var history2 = new ArticleNameHistory { Id = Guid.NewGuid(), ArticleId = article.Id, LanguageId = "en", ChangeDate = DateTime.Now };

            ctx.AttachSingle(history2);

            var name2 = new ArticleName { ArticleId = article.Id, LanguageId = "en", TranlatedText = "second" };

            ctx.AttachSingle(name2);

            Assert.NotNull(name2.Article);
            Assert.NotNull(history2.ArticleName);
            Assert.Contains(name2.History, p => p == history2);
            Assert.Contains(article.Names, p => p == name2);
        }

        [Fact]
        public void DontUpdateIfModifiedTest()
        {
            var model = GetModel();

            var context = new ModelContext(model);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                State = TrackingState.Modified,
                ReceiptNumber = "11111111"
            };

            var invoiceNew = new Invoice
            {
                Id = invoice.Id,
                State = TrackingState.Unchanged,
                ReceiptNumber = "22222222"
            };

            var attached1 = context.AttachSingle(invoice);
            Assert.Equal("11111111", attached1.ReceiptNumber);
            Assert.Equal(invoice, attached1);

            var attached2 = context.AttachSingle(invoiceNew);
            Assert.Equal(invoice, attached2);
            Assert.Equal("11111111", attached2.ReceiptNumber);
        }

        [Fact]
        public void ForceUpdateTest()
        {
            var model = GetModel();

            var context = new ModelContext(model);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                State = TrackingState.Modified,
                ReceiptNumber = "11111111"
            };

            var invoiceNew = new Invoice
            {
                Id = invoice.Id,
                State = TrackingState.Unchanged,
                ReceiptNumber = "22222222"
            };

            var attached1 = context.AttachSingle(invoice);
            Assert.Equal("11111111", attached1.ReceiptNumber);
            Assert.Equal(invoice, attached1);

            var attached2 = context.AttachSingle(invoiceNew, MergeStrategy.ForceUpdate);
            Assert.Equal(invoice, attached2);
            Assert.Equal("22222222", attached2.ReceiptNumber);
            Assert.Equal(TrackingState.Unchanged, attached2.State);
        }

        [Fact]
        public void GetChangesTest()
        {
            MetadataModel model = GetModel();

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

        [Fact]
        public void MassDetachTest()
        {
            var model = GetModel();
            var context = new ModelContext(model);

            var rand = new Random();

            var customers = Enumerable.Range(0, 1000)
                                .Select(p => new Contact { Id = Guid.NewGuid(), ContactType = ContactType.Customer, Receipts = new HashSet<Receipt>(), State = TrackingState.Added, Street = $"Street {p}", LastName = $"LastName {p}", FirstName = $"FirstName {p}" })
                                .ToList();

            var invoices = Enumerable.Range(0, 20000)
                                .Select(p => new Invoice { Id = Guid.NewGuid(), CustomerId = customers[rand.Next(999)].Id, ReceiptNumber = p.ToString(), State = TrackingState.Added, LastPaymentDate = DateTime.Now.AddHours(-1 * rand.Next(4000)) })
                                .ToList();

            var lines = Enumerable.Range(0, 200000)
                                .Select(p => new ReceiptDetail { Id = Guid.NewGuid(), Amount = rand.Next(), Description = $"Description {p}", State = TrackingState.Added, ReceiptId = invoices[rand.Next(0, 19999)].Id })
                                .ToList();

            context.Attach(customers);
            context.Attach(invoices);
            context.Attach(lines);

            Assert.Equal(221000, context.TrackedObjects.Count());

            context.Detach(invoices);

            Assert.Equal(201000, context.TrackedObjects.Count());
        }

        [Fact]
        public void UpdateIfUnchangedTest()
        {
            var model = GetModel();

            var context = new ModelContext(model);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                State = TrackingState.Unchanged,
                ReceiptNumber = "11111111"
            };

            var invoiceNew = new Invoice
            {
                Id = invoice.Id,
                State = TrackingState.Unchanged,
                ReceiptNumber = "22222222"
            };

            var attached1 = context.AttachSingle(invoice);
            Assert.Equal("11111111", attached1.ReceiptNumber);
            Assert.Equal(invoice, attached1);

            var attached2 = context.AttachSingle(invoiceNew);
            Assert.Equal(invoice, attached2);
            Assert.Equal("22222222", attached2.ReceiptNumber);
        }

        private static MetadataModel GetModel()
        {
            var metadataModelBuilder = new MetadataModelBuilder();
            metadataModelBuilder.Exclude<EntityBase>();
            var articleNameEntity = metadataModelBuilder.Entity<ArticleName>();
            articleNameEntity.PrimaryKey.AddRange(new[] { "ArticleId", "LanguageId" });


            var articleEntity = metadataModelBuilder.Entity<Article>();
            metadataModelBuilder.Entity<ReceiptDetail>();
            metadataModelBuilder.Entity<Invoice>().BaseEntity = metadataModelBuilder.Entity(typeof(Receipt));
            var contactSettings = metadataModelBuilder.Entity<ContactSettings>();

            var articleNameHistoryEntity = metadataModelBuilder.Entity<ArticleNameHistory>();
            articleNameHistoryEntity.HasOne(p => p.ArticleName).WithMany(p => p.History).HasForeignKey("ArticleId", "LanguageId");

            articleNameEntity.HasMany(p => p.History).WithOne(p => p.ArticleName).HasForeignKey("ArticleId", "LanguageId");

            articleEntity.HasOne(p => p.ArticleSettings).WithPrincipal();
            contactSettings.HasOne(p => p.Contact).WithDependant();

            metadataModelBuilder.ApplyConventions();

            var model = metadataModelBuilder.ToModel();
            return model;
        }
    }
}