using System;
using System.Linq;
using Lucile.Test.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tests;

namespace Lucile.EntityFrameworkCore.Test
{
    public class TestContext : DbContext
    {
        public TestContext()
        {

        }

        public TestContext(DbContextOptions<TestContext> options)
            : base(options)
        {

        }

        public DbSet<AllTypesEntity> AllTypesEntity { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<ChangeEntry<Guid, Guid>> ChangeEntryGuidGuids { get; set; }

        public DbSet<ChangeEntry<Guid>> ChangeEntryGuids { get; set; }

        public DbSet<ChangeEntry<string>> ChangeEntryStrings { get; set; }

        public DbSet<ChangeVersion> ChangeVersions { get; set; }

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
            modelBuilder.Entity<ArticleStatistics>().HasNoKey();

            modelBuilder.Entity<Order>();
            modelBuilder.Entity<Invoice>();

            modelBuilder.Entity<Contact>().HasOne<ContactSettings>().WithOne(p => p.Contact).HasPrincipalKey<Contact>(p => p.Id);

            modelBuilder.Entity<Receipt>().Property(p => p.ReceiptDate).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Receipt>().Property(p => p.ReceiptType).HasDefaultValue(ReceiptType.Offer);

            var articleEntity = modelBuilder.Entity<Article>();

            articleEntity.HasOne(p => p.ArticleSettings).WithOne().HasPrincipalKey<Article>(p => p.Id);
            articleEntity.Property<string>("CreatedBy").HasMaxLength(20);

            modelBuilder.Entity<ArticleName>().HasKey(p => new { p.ArticleId, p.LanguageId });

            modelBuilder.Entity<ArticleNameHistory>()
                .HasOne(p => p.ArticleName).WithMany(p => p.History)
                .HasForeignKey(p => new { p.ArticleId, p.LanguageId });

            modelBuilder.Model.GetEntityTypes()
                .SelectMany(p => p.GetProperties())
                .Where(p => p.ClrType == typeof(Guid))
                .ToList()
                .ForEach(p => p.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);

            var entry = modelBuilder.Entity(typeof(ChangeEntry<Guid>));
            entry.HasBaseType((Type)null);
            entry.HasKey("Version", "Key1");

            entry.HasOne(typeof(ChangeVersion), "ChangeVersion")
                .WithMany()
                .HasForeignKey("Version");

            entry = modelBuilder.Entity(typeof(ChangeEntry<string>));
            entry.HasBaseType((Type)null);
            entry.HasKey("Version", "Key1");

            entry.HasOne(typeof(ChangeVersion), "ChangeVersion")
                .WithMany()
                .HasForeignKey("Version");

            entry = modelBuilder.Entity(typeof(ChangeEntry<Guid, Guid>));
            entry.HasBaseType((Type)null);
            entry.HasKey("Version", "Key1", "Key2");

            entry.HasOne(typeof(ChangeVersion), "ChangeVersion")
                .WithMany()
                .HasForeignKey("Version");


            foreach (var item in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var nav in item.GetNavigations())
                {
                    nav.ForeignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}