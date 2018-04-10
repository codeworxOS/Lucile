using System;
using System.Linq;
using Lucile.Test.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lucile.EntityFrameworkCore.Test
{
    public class TestContext : DbContext
    {
        public DbSet<AllTypesEntity> AllTypesEntity { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<ReceiptDetail> ReceiptDetails { get; set; }

        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("unittest");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>();
            modelBuilder.Entity<Invoice>();

            modelBuilder.Entity<Contact>().HasOne<ContactSettings>().WithOne(p => p.Contact).HasPrincipalKey<Contact>(p => p.Id);

            modelBuilder.Entity<Article>().HasOne(p => p.ArticleSettings).WithOne().HasPrincipalKey<Article>(p => p.Id);

            modelBuilder.Entity<ArticleName>().HasKey(p => new { p.ArticleId, p.LanguageId });

            modelBuilder.Model.GetEntityTypes()
                .SelectMany(p => p.GetProperties())
                .Where(p => p.ClrType == typeof(Guid))
                .ToList()
                .ForEach(p => p.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
        }
    }
}