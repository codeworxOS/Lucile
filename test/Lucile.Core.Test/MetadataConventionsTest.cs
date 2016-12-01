using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class MetadataConventionsTest
    {
        [Fact]
        public void DefaultKeyConventionTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Test1>().HasOne(p => p.Test2).WithPrincipal(p => p.Test1);
            builder.ApplyConventions();
            var entity = builder.Entity<Test1>();
            var entity2 = builder.Entity<Test2>();

            var nav1 = entity.Navigation("Test2");
            var nav2 = entity2.Navigation("Test1");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, nav1.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, nav1.TargetMultiplicity);

            Assert.Equal(NavigationPropertyMultiplicity.One, nav2.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, nav2.TargetMultiplicity);
        }

        [Fact]
        public void DefaultStructureConventionTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>();
            builder.Entity<Article>().HasOne(p => p.ArticleSettings).WithPrincipal();
            builder.ApplyConventions();

            Assert.Equal(8, builder.Entities.Count());

            Assert.Contains(builder.Entities, p => p.Name == "Invoice");
            Assert.Contains(builder.Entities, p => p.Name == "Receipt");
            Assert.Contains(builder.Entities, p => p.Name == "ReceiptDetail");
            Assert.Contains(builder.Entities, p => p.Name == "Article");
            Assert.Contains(builder.Entities, p => p.Name == "ArticleName");
            Assert.Contains(builder.Entities, p => p.Name == "ArticleSettings");
            Assert.Contains(builder.Entities, p => p.Name == "Contact");
            Assert.Contains(builder.Entities, p => p.Name == "Country");

            Assert.Equal(5, builder.Entity<Article>().Properties.Count());

            Assert.Contains(builder.Entity<Article>().Properties, p => p.Name == "Id");
            Assert.Contains(builder.Entity<Article>().Properties, p => p.Name == "ArticleDescription");
            Assert.Contains(builder.Entity<Article>().Properties, p => p.Name == "ArticleNumber");
            Assert.Contains(builder.Entity<Article>().Properties, p => p.Name == "Price");
            Assert.Contains(builder.Entity<Article>().Properties, p => p.Name == "SupplierId");

            Assert.Equal(3, builder.Entity<Article>().Navigations.Count());
            Assert.Contains(builder.Entity<Article>().Navigations, p => p.Name == "ArticleSettings");
            Assert.Contains(builder.Entity<Article>().Navigations, p => p.Name == "Names");
            Assert.Contains(builder.Entity<Article>().Navigations, p => p.Name == "Supplier");

            var settings = builder.Entity<Article>().Navigation("ArticleSettings");
            var names = builder.Entity<Article>().Navigation("Names");
            var supplier = builder.Entity<Article>().Navigation("Supplier");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, settings.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, settings.TargetMultiplicity);

            Assert.Equal(NavigationPropertyMultiplicity.Many, names.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.One, names.TargetMultiplicity);
            Assert.Equal("Article", names.TargetProperty);

            Assert.Equal(NavigationPropertyMultiplicity.One, supplier.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, supplier.TargetMultiplicity);
            Assert.Equal("Articles", supplier.TargetProperty);

            var contact = builder.Entity<Contact>();
            var country = contact.Navigation("Country");

            Assert.Equal(NavigationPropertyMultiplicity.One, country.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, country.TargetMultiplicity);
            Assert.Null(country.TargetProperty);

            var receiptDetail = builder.Entity<ReceiptDetail>();
            var article = receiptDetail.Navigation("Article");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, article.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, article.TargetMultiplicity);

            Assert.Null(article.TargetProperty);
        }

        [Fact]
        public void OneToOneDefaultStructureConventionTest()
        {
            var builder = new MetadataModelBuilder();
            var entity = builder.Entity<Test1>();
            builder.ApplyConventions();
            var entity2 = builder.Entity<Test2>();

            var nav1 = entity.Navigation("Test2");
            var nav2 = entity2.Navigation("Test1");

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, nav1.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, nav1.TargetMultiplicity);

            Assert.Equal(NavigationPropertyMultiplicity.ZeroOrOne, nav2.Multiplicity);
            Assert.Equal(NavigationPropertyMultiplicity.Many, nav2.TargetMultiplicity);
        }

        [Fact]
        public void OneToOneWithConfigDefaultStructureConventionTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Test1>().HasOne(p => p.Test2).WithPrincipal(p => p.Test1);
            builder.ApplyConventions();
            EntityMetadataBuilder entity = builder.Entity<Test1>();

            Assert.Equal(1, entity.PrimaryKey.Count);
            Assert.Equal("Id", entity.PrimaryKey.First());

            entity = builder.Entity<Test2>();

            Assert.Equal(1, entity.PrimaryKey.Count);
            Assert.Equal("Test2ID", entity.PrimaryKey.First());
        }

        private class Test1
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public Test2 Test2 { get; set; }
        }

        private class Test2
        {
            public Test1 Test1 { get; set; }

            public int Test2ID { get; set; }

            public string Testchen { get; set; }
        }
    }
}