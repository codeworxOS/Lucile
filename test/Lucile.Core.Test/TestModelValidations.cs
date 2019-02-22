using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using System.Linq;
using Xunit;

namespace Tests
{
    public static class TestModelValidations
    {
        public static void ValidateInvoiceArticleDefaultModel(MetadataModelBuilder builder)
        {
            var model = builder.ToModel();
            ValidateInvoiceArticleDefaultModel(model);
        }

        public static void ValidateInvoiceArticleDefaultModel(MetadataModel model)
        {
            Assert.Equal(8, model.Entities.Count());

            Assert.Contains(model.Entities, p => p.Name == "Invoice");
            Assert.Contains(model.Entities, p => p.Name == "Receipt");
            Assert.Contains(model.Entities, p => p.Name == "ReceiptDetail");
            Assert.Contains(model.Entities, p => p.Name == "Article");
            Assert.Contains(model.Entities, p => p.Name == "ArticleName");
            Assert.Contains(model.Entities, p => p.Name == "ArticleSettings");
            Assert.Contains(model.Entities, p => p.Name == "Contact");
            Assert.Contains(model.Entities, p => p.Name == "Country");

            Assert.Equal(5, model.GetEntityMetadata<Article>().GetProperties().Count());

            Assert.Contains(model.GetEntityMetadata<Article>().GetProperties(), p => p.Name == "Id");
            Assert.Contains(model.GetEntityMetadata<Article>().GetProperties(), p => p.Name == "ArticleDescription");
            Assert.Contains(model.GetEntityMetadata<Article>().GetProperties(), p => p.Name == "ArticleNumber");
            Assert.Contains(model.GetEntityMetadata<Article>().GetProperties(), p => p.Name == "Price");
            Assert.Contains(model.GetEntityMetadata<Article>().GetProperties(), p => p.Name == "SupplierId");

            Assert.Equal(3, model.GetEntityMetadata<Article>().GetNavigations().Count());
            Assert.Contains(model.GetEntityMetadata<Article>().GetNavigations(), p => p.Name == "ArticleSettings");
            Assert.Contains(model.GetEntityMetadata<Article>().GetNavigations(), p => p.Name == "Names");
            Assert.Contains(model.GetEntityMetadata<Article>().GetNavigations(), p => p.Name == "Supplier");

            var settings = model.GetEntityMetadata<Article>().GetNavigations().First(p => p.Name == "ArticleSettings");
            var names = model.GetEntityMetadata<Article>().GetNavigations().First(p => p.Name == "Names");
            var supplier = model.GetEntityMetadata<Article>().GetNavigations().First(p => p.Name == "Supplier");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, settings.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, settings.TargetMultiplicity);

            Assert.Equal(NavigationPropertyMultiplicity.Many, names.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, names.TargetMultiplicity);
            Assert.Equal("Article", names.TargetNavigationProperty.Name);

            Assert.Equal(NavigationPropertyMultiplicity.One, supplier.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, supplier.TargetMultiplicity);
            Assert.Equal("Articles", supplier.TargetNavigationProperty.Name);

            var contact = model.GetEntityMetadata<Contact>();
            var country = contact.GetNavigations().First(p => p.Name == "Country");

            Assert.Equal(NavigationPropertyMultiplicity.One, country.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, country.TargetMultiplicity);
            Assert.Null(country.TargetNavigationProperty);

            var receiptDetail = model.GetEntityMetadata<ReceiptDetail>();
            var article = receiptDetail.GetNavigations().First(p => p.Name == "Article");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, article.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, article.TargetMultiplicity);

            Assert.Null(article.TargetNavigationProperty);
        }
    }
}