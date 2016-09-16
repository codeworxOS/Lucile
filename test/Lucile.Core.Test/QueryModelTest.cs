using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lucile.Core.Test.Model;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Xunit;

namespace Tests
{
    public class QueryModelTest
    {
        [Fact]
        public void PropertyConfigurationBuilderConstructorTest()
        {
            var test = new ReceiptDetail();

            var descriptionBuilder = new PropertyConfigurationBuilder<ReceiptDetail, string>(p => p.Description);
            Assert.Equal(nameof(test.Description), descriptionBuilder.PropertyName);
            Assert.Equal(typeof(string), descriptionBuilder.PropertyType);
            var amountBuilder = new PropertyConfigurationBuilder<ReceiptDetail, decimal>(p => p.Amount);
            Assert.Equal(nameof(test.Amount), amountBuilder.PropertyName);
            Assert.Equal(typeof(decimal), amountBuilder.PropertyType);
            var receiptBuilder = new PropertyConfigurationBuilder<ReceiptDetail, Receipt>(p => p.Receipt);
            Assert.Equal(nameof(test.Receipt), receiptBuilder.PropertyName);
            Assert.Equal(typeof(Receipt), receiptBuilder.PropertyType);
            var articleIdBuilder = new PropertyConfigurationBuilder<ReceiptDetail, Guid?>(p => p.ArticleId);
            Assert.Equal(nameof(test.ArticleId), articleIdBuilder.PropertyName);
            Assert.Equal(typeof(Guid?), articleIdBuilder.PropertyType);

            Assert.Throws(typeof(ArgumentException), () => new PropertyConfigurationBuilder<ReceiptDetail, string>(p => p.Amount.ToString()));
            Assert.Throws(typeof(ArgumentException), () => new PropertyConfigurationBuilder<ReceiptDetail, int>(p => p.Amount.ToString().Length));
        }

        [Fact]
        public void PropertyConfigurationDependenciesTest()
        {
            var builder = QueryModel.Build(
                            p => new
                            {
                                ReceiptDetail = p.Get<ReceiptDetail>(),
                                CustomerStatistics = p.Get<CustomerStatistics>(),
                                ArticleStatistics = p.Get<ArticleStatistics>(),
                                Whatever = p.Get<Whatever>()
                            },
                            p => new
                            {
                                Id = p.ReceiptDetail.Id,
                                Price = p.ReceiptDetail.Price,
                                SoldLastMonth = p.ArticleStatistics.SoldLastMonth,
                                LastPurchase = p.CustomerStatistics.LastPurchase,
                                Whatever = p.Whatever.ArticleNumber + "-" + p.ReceiptDetail.Receipt.ReceiptNumber
                            });
            builder.HasKey(p => p.Id);
            builder.Source(p => p.ArticleStatistics)
                .Join(p => p.ArticleId, p => p.ReceiptDetail.ArticleId);
            builder.Source(p => p.CustomerStatistics)
                .Join(p => p.CustomerId, p => p.ReceiptDetail.Receipt.CustomerId);
            builder.Source(p => p.Whatever)
                .Join(p => new
                {
                    LastDate = p.LastPurchaseDate,
                    ArticleNumber = p.ArticleNumber
                },
                p => new
                {
                    LastDate = p.CustomerStatistics.LastPurchase,
                    ArticleNumber = p.ArticleStatistics.ArticleNumber
                });

            var model = builder.ToModel();

            var idProp = model.PropertyConfigurations.First(p => p.Property.Name == "Id");
            var priceProp = model.PropertyConfigurations.First(p => p.Property.Name == "Price");
            var soldLastMonthProp = model.PropertyConfigurations.First(p => p.Property.Name == "SoldLastMonth");
            var lastPurchaseProp = model.PropertyConfigurations.First(p => p.Property.Name == "LastPurchase");
            var whateverProp = model.PropertyConfigurations.First(p => p.Property.Name == "Whatever");

            Assert.Equal<string>(new string[] { "ReceiptDetail" }, idProp.DependsOn);
            Assert.Equal<string>(new string[] { "ReceiptDetail" }, priceProp.DependsOn);
            Assert.Equal<string>(new string[] { "ArticleStatistics" }, soldLastMonthProp.DependsOn);
            Assert.Equal<string>(new string[] { "CustomerStatistics" }, lastPurchaseProp.DependsOn);
            Assert.Equal<string>(new string[] { "Whatever", "ReceiptDetail" }, whateverProp.DependsOn);
        }

        [Fact]
        public void QueryModelBuilderConstructorTest()
        {
            var builder1 = QueryModel.Build(p => p.Get<ReceiptDetail>(), p => new { Amount = p.Amount });
            Assert.True(builder1.IsSingleSourceQuery);

            Assert.Throws<ArgumentException>(() => QueryModel.Build(p => default(ReceiptDetail), p => new { Amount = p.Amount }));

            var builder2 = QueryModel.Build(p => new { ReceiptDetail = p.Get<ReceiptDetail>() }, p => new { Amount = p.ReceiptDetail.Amount });
            Assert.False(builder2.IsSingleSourceQuery);

            Assert.Throws<ArgumentException>(() => QueryModel.Build(p => new { ReceiptDetail = new ReceiptDetail() }, p => new { Amount = p.ReceiptDetail.Amount }));

            var builder3 = QueryModel.Build(p => new SourceClass { ReceiptDetail = p.Get<ReceiptDetail>() }, p => new AmountResult { Amount = p.ReceiptDetail.Amount });
            Assert.False(builder3.IsSingleSourceQuery);
        }

        [Fact]
        public void QueryModelBuilderNonGenericSourceMethodTest()
        {
            var builder = QueryModel.Build(p =>
            new
            {
                ReceiptDetail = p.Get<ReceiptDetail>()
            },
            p => new
            {
                Amount = p.ReceiptDetail.Amount
            });

            var source = builder.Source("ReceiptDetail");

            var source2 = builder.Source(p => p.ReceiptDetail);
            Assert.NotNull(source);
            Assert.Equal(source, source2);

            Assert.Throws<ArgumentException>(() => builder.Source("ABC"));
        }

        [Fact]
        public void QueryModelBuildHasKeyMemberExpressionTest()
        {
            var builder = QueryModel.Build(
                            p => p.Get<ReceiptDetail>(),
                            p => new
                            {
                                BlaId = p.Id,
                                Test = p.Price,
                                Test2 = p.Receipt.ReceiptNumber
                            });
            builder.HasKey(p => p.BlaId);

            Assert.True(builder.Property(p => p.BlaId).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test2).IsPrimaryKey);
        }

        [Fact]
        public void QueryModelBuildHasNewExpressionTest()
        {
            var builder = QueryModel.Build(
                            p => p.Get<ReceiptDetail>(),
                            p => new
                            {
                                ReceiptId = p.Receipt.Id,
                                DetailId = p.Id,
                                Test = p.Price,
                                Test2 = p.Receipt.ReceiptNumber
                            });
            builder.HasKey(p => new
            {
                p.ReceiptId,
                p.DetailId
            });

            Assert.True(builder.Property(p => p.ReceiptId).IsPrimaryKey);
            Assert.True(builder.Property(p => p.DetailId).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test2).IsPrimaryKey);
        }

        [Fact]
        public void QueryModelBuildHasNewInitExpressionTest()
        {
            var builder = QueryModel.Build(
                            p => p.Get<ReceiptDetail>(),
                            p => new
                            {
                                ReceiptId = p.Receipt.Id,
                                DetailId = p.Id,
                                Test = p.Price,
                                Test2 = p.Receipt.ReceiptNumber
                            });
            builder.HasKey(p => new
            {
                MasterId = p.ReceiptId,
                DetailId = p.DetailId
            });

            Assert.True(builder.Property(p => p.ReceiptId).IsPrimaryKey);
            Assert.True(builder.Property(p => p.DetailId).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test).IsPrimaryKey);
            Assert.False(builder.Property(p => p.Test2).IsPrimaryKey);
        }

        [Fact]
        public async Task QueryModelBuildSimpleAsync()
        {
            var builder = QueryModel.Build(
                p => new
                {
                    ReceiptDetail = p.Get<ReceiptDetail>(),
                    CustomerStatistics = p.Get<CustomerStatistics>(),
                    ArticleStatistics = p.Get<ArticleStatistics>()
                },
                p => new
                {
                    ReceiptDetailId = p.ReceiptDetail.Id,
                    ReceiptNumber = p.ReceiptDetail.Receipt.ReceiptNumber,
                    ArticleNumber = p.ReceiptDetail.Article.ArticleNumber,
                    ArticleSoldYear = p.ArticleStatistics.SoldCurrentYear,
                    ArticleSoldMonth = p.ArticleStatistics.SoldCurrentMonth,
                    ArticleSoldLastMonth = p.ArticleStatistics.SoldLastMonth,
                    ArticleRevenueYear = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldCurrentYear,
                    ArticleRevenueMonth = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldCurrentMonth,
                    ArticleRevenueLastMonth = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldLastMonth,
                    Supplier = p.ReceiptDetail.Article.Supplier.FirstName + " " + p.ReceiptDetail.Article.Supplier.LastName,
                    Customer = p.ReceiptDetail.Receipt.Customer.FirstName + " " + p.ReceiptDetail.Receipt.Customer.LastName + " (" + p.ReceiptDetail.Receipt.Customer.Identity + ")",
                    CustomerRevenueYear = p.CustomerStatistics.ReceiptAmountCurrentYear,
                    CustomerRevenueMonth = p.CustomerStatistics.ReceiptAmountCurrentMonth,
                    CustomerRevenueLastMonth = p.CustomerStatistics.ReceiptAmountLastMonth,
                    CustomerLastPurchase = p.CustomerStatistics.LastPurchase
                });

            builder.Source(p => p.CustomerStatistics)
                .Query(p =>
                        from rd in p.Query<ReceiptDetail>()
                        group rd by rd.Receipt.CustomerId into grp
                        join y in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals y.Key into tmpy
                        from year in tmpy.DefaultIfEmpty()
                        join cm in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals cm.Key into tmpcm
                        from currentMonth in tmpcm.DefaultIfEmpty()
                        join lm in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals lm.Key into tmplm
                        from lastMonth in tmplm.DefaultIfEmpty()
                        select new CustomerStatistics
                        {
                            CustomerId = grp.Key,
                            LastPurchase = grp.Max(x => x.Receipt.ReceiptDate),
                            ReceiptAmountCurrentYear = year.Sum(x => x.Amount),
                            ReceiptAmountCurrentMonth = currentMonth.Sum(x => x.Amount),
                            ReceiptAmountLastMonth = lastMonth.Sum(x => x.Amount)
                        }
                      )
                      .Join(p => p.CustomerId, p => p.ReceiptDetail.Receipt.CustomerId);

            builder.Source(p => p.ArticleStatistics)
                .Query(p =>
                        from rd in p.Query<ReceiptDetail>()
                        where rd.ArticleId.HasValue
                        group rd by rd.ArticleId.Value into grp
                        join y in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals y.Key into tmpy
                        from year in tmpy.DefaultIfEmpty()
                        join cm in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals cm.Key into tmpcm
                        from currentMonth in tmpcm.DefaultIfEmpty()
                        join lm in p.Query<ReceiptDetail>().Where(x => x.Receipt.ReceiptDate.Year == DateTime.Today.Year).GroupBy(x => x.Receipt.CustomerId) on grp.Key equals lm.Key into tmplm
                        from lastMonth in tmplm.DefaultIfEmpty()
                        select new ArticleStatistics
                        {
                            ArticleId = grp.Key,
                            SoldCurrentYear = year.Sum(x => x.Amount),
                            SoldCurrentMonth = year.Sum(x => x.Amount),
                            SoldLastMonth = year.Sum(x => x.Amount)
                        }
                      )
                      .Join(p => p.ArticleId, p => p.ReceiptDetail.ArticleId);

            var qs = new DummyQuerySource();

            var test = from rd in qs.Query<ReceiptDetail>()
                       join artstat in qs.Query<ArticleStatistics>() on rd.ArticleId equals artstat.ArticleId into tmpArtStat
                       from ast in tmpArtStat.DefaultIfEmpty()
                       join custstat in qs.Query<CustomerStatistics>() on rd.Receipt.CustomerId equals custstat.CustomerId into tmpCustStat
                       from cst in tmpCustStat.DefaultIfEmpty()
                       select new
                       {
                           ReceiptDetail = rd,
                           ArticleStatistics = ast,
                           CustomerStatistics = cst
                       };

            var model = builder.ToModel();

            Assert.NotNull(model);

            var selectItems = model.ResultType.GetProperties().OrderBy(p => p.Name).Skip(2).Select(p => new SelectItem(p.Name, Aggregate.None));

            var config = new QueryConfiguration(selectItems, Enumerable.Empty<SortItem>());
            var query = model.GetQuery(new DummyQuerySource(), config);

            var result = query.Cast<object>().ToList();

            Assert.NotEmpty(result);
        }

        [Fact]
        public void QueryModelSourceDependenciesTest()
        {
            var builder = QueryModel.Build(
                            p => new
                            {
                                ReceiptDetail = p.Get<ReceiptDetail>(),
                                CustomerStatistics = p.Get<CustomerStatistics>(),
                                ArticleStatistics = p.Get<ArticleStatistics>(),
                                Whatever = p.Get<Whatever>()
                            },
                            p => new
                            {
                                Id = p.ReceiptDetail.Id,
                                Price = p.ReceiptDetail.Price,
                                SoldLastMonth = p.ArticleStatistics.SoldLastMonth,
                                LastPurchase = p.CustomerStatistics.LastPurchase,
                                Whatever = p.Whatever.WhatEverInfo
                            });
            builder.HasKey(p => p.Id);
            builder.Source(p => p.ArticleStatistics)
                .Join(p => p.ArticleId, p => p.ReceiptDetail.ArticleId);
            builder.Source(p => p.CustomerStatistics)
                .Join(p => p.CustomerId, p => p.ReceiptDetail.Receipt.CustomerId);
            builder.Source(p => p.Whatever)
                .Join(p => new
                {
                    LastDate = p.LastPurchaseDate,
                    ArticleNumber = p.ArticleNumber
                },
                p => new
                {
                    LastDate = p.CustomerStatistics.LastPurchase,
                    ArticleNumber = p.ArticleStatistics.ArticleNumber
                });

            var model = builder.ToModel();
            var receiptDetail = model.SourceEntityConfigurations.First(p => p.Name == "ReceiptDetail");
            var articleStatistics = model.SourceEntityConfigurations.First(p => p.Name == "ArticleStatistics");
            var customerStatistics = model.SourceEntityConfigurations.First(p => p.Name == "CustomerStatistics");
            var whatever = model.SourceEntityConfigurations.First(p => p.Name == "Whatever");

            Assert.Equal(receiptDetail.DependsOn, new string[] { });
            Assert.Equal(articleStatistics.DependsOn, new string[] { "ReceiptDetail" });
            Assert.Equal(customerStatistics.DependsOn, new string[] { "ReceiptDetail" });
            Assert.Equal(whatever.DependsOn.OrderBy(p => p), new string[] { "ArticleStatistics", "CustomerStatistics" });
        }

        [Fact]
        public void SourceEnittyConfigurationConstructorTest()
        {
            var builder = QueryModel.Build(
               p => new
               {
                   ReceiptDetail = p.Get<ReceiptDetail>(),
                   CustomerStatistics = p.Get<CustomerStatistics>(),
                   ArticleStatistics = p.Get<ArticleStatistics>()
               },
               p => new
               {
                   ReceiptDetailId = p.ReceiptDetail.Id,
                   ReceiptNumber = p.ReceiptDetail.Receipt.ReceiptNumber,
                   ArticleNumber = p.ReceiptDetail.Article,
                   ArticleSoldYear = p.ArticleStatistics.SoldCurrentYear,
                   ArticleSoldMonth = p.ArticleStatistics.SoldCurrentMonth,
                   ArticleSoldLastMonth = p.ArticleStatistics.SoldLastMonth,
                   ArticleRevenueYear = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldCurrentYear,
                   ArticleRevenueMonth = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldCurrentMonth,
                   ArticleRevenueLastMonth = p.ReceiptDetail.Article.Price * p.ArticleStatistics.SoldLastMonth,
                   Supplier = p.ReceiptDetail.Article.Supplier.FirstName + " " + p.ReceiptDetail.Article.Supplier.LastName,
                   Customer = p.ReceiptDetail.Receipt.Customer.FirstName + " " + p.ReceiptDetail.Receipt.Customer.LastName + " (" + p.ReceiptDetail.Receipt.Customer.Identity + ")",
                   CustomerRevenueYear = p.CustomerStatistics.ReceiptAmountCurrentYear,
                   CustomerRevenueMonth = p.CustomerStatistics.ReceiptAmountCurrentMonth,
                   CustomerRevenueLastMonth = p.CustomerStatistics.ReceiptAmountLastMonth,
                   CustomerLastPurchase = p.CustomerStatistics.LastPurchase
               });

            builder.Source(p => p.CustomerStatistics)
                .Join(p => p.CustomerId, p => p.ReceiptDetail.Receipt.CustomerId);

            builder.Source(p => p.ArticleStatistics)
                .Join(p => p.ArticleId, p => p.ReceiptDetail.ArticleId);

            var source1 = builder.Source(p => p.ReceiptDetail);
            var source2 = builder.Source(p => p.ArticleStatistics);
            var source3 = builder.Source(p => p.CustomerStatistics);
            var toTest1 = new SourceEntityConfiguration("ReceiptDetail", typeof(ReceiptDetail), source1.QueryFactory, source1.JoinKeyType, source1.LocalJoinExpression, source1.RemoteJoinExpression);
            var toTest2 = new SourceEntityConfiguration("ArticleStatistics", typeof(ArticleStatistics), source2.QueryFactory, source2.JoinKeyType, source2.LocalJoinExpression, source2.RemoteJoinExpression);
            var toTest3 = new SourceEntityConfiguration("CustomerStatistics", typeof(CustomerStatistics), source3.QueryFactory, source3.JoinKeyType, source3.LocalJoinExpression, source3.RemoteJoinExpression);

            Assert.True(toTest1.IsRootQuery);
            Assert.Empty(toTest1.DependsOn);

            Assert.False(toTest2.IsRootQuery);
            Assert.Contains(toTest2.DependsOn, p => p == "ReceiptDetail");
            Assert.Equal(1, toTest2.DependsOn.Count);

            Assert.False(toTest3.IsRootQuery);
            Assert.Contains(toTest3.DependsOn, p => p == "ReceiptDetail");
            Assert.Equal(1, toTest3.DependsOn.Count);
        }

        private class AmountResult
        {
            public decimal Amount { get; set; }
        }

        private class SourceClass
        {
            public ReceiptDetail ReceiptDetail { get; set; }
        }

        private class Whatever
        {
            public string ArticleNumber { get; internal set; }

            public DateTime LastPurchaseDate { get; internal set; }

            public string WhatEverInfo { get; set; }
        }
    }
}