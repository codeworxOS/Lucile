using System;
using System.Data.Entity;
using System.Linq;
using Lucile.Test.Model;

namespace Lucile.EntityFramework.Test
{
    public class TestContext : DbContext
    {
        static TestContext()
        {
            Database.SetInitializer<TestContext>(null);
        }

        public DbSet<AllTypesEntity> AllTypesEntity { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<EntityBase>();
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<Invoice>();

            modelBuilder.Entity<ContactSettings>().HasRequired(p => p.Contact).WithOptional();
            var allTypes = modelBuilder.Entity<AllTypesEntity>();
            allTypes.Ignore(p => p.SByteProperty);
            allTypes.Ignore(p => p.NullableSByteProperty);

            modelBuilder.Entity<Article>().HasOptional(p => p.ArticleSettings).WithRequired();

            modelBuilder.Entity<ArticleName>().HasKey(p => new { p.ArticleId, p.LanguageId });
        }
    }
}