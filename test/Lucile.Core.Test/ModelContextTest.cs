using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucile.Data;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
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
            metadataModelBuilder.Entity<Invoice>();
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
    }
}