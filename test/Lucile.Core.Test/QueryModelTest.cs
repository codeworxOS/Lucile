using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Lucile.Linq.Configuration.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class QueryModelTest
    {
        [Fact]
        public void AnyFilterItemTest()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var receipt1 = new Invoice
            {
                Id = id1,
                Details = {
                    new ReceiptDetail { Price = 101 }
                }
            };

            var receipt2 = new Invoice
            {
                Id = id2,
                Details = {
                    new ReceiptDetail { Price = 100 }
                }
            };

            var receipts = new List<Receipt>() {
                receipt1,
                receipt2
            };

            var filterItem = new AnyFilterItemBuilder()
            {
                Path = "Details",
                Filter = new NumericFilterItemBuilder
                {
                    Left = new PathValueExpressionBuilder { Path = "Price" },
                    Operator = RelationalCompareOperator.GreaterThen,
                    Right = new NumericConstantValueBuilder { Value = 100 }
                }
            };

            var query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(id1, query.First().Id);

            ((NumericFilterItemBuilder)filterItem.Filter).Operator = RelationalCompareOperator.LessThenOrEqual;

            query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(id2, query.First().Id);
        }

        [Fact]
        public void GuidFilterItemTest()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var receipt1 = new Invoice
            {
                Id = id1,
            };

            var receipt2 = new Invoice
            {
                Id = id2,
            };

            var receipts = new List<Receipt>() {
                receipt1,
                receipt2
            };

            var filterItem = new GuidFilterItemBuilder
            {
                Left = new PathValueExpressionBuilder { Path = "Id" },
                Operator = RelationalCompareOperator.Equal,
                Right = new GuidConstantValueBuilder { Value = id1 }
            };

            var query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(receipt1, query.First());

            ((GuidConstantValueBuilder)filterItem.Right).Value = id2;

            query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(receipt2, query.First());
        }

        [Fact]
        public void NullFilterItemTest()
        {
            var builder = new DateTimeFilterItemBuilder
            {
                Left = new PathValueExpressionBuilder { Path = "ExpectedDeliveryDate" },
                Operator = RelationalCompareOperator.IsNull
            };

            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();
            Guid id3 = Guid.NewGuid();

            var receipts = new[] {
                 new Invoice{ Id = id1, ExpectedDeliveryDate = DateTime.Today },
                 new Invoice{ Id = id2, ExpectedDeliveryDate = DateTime.Today.AddDays(1) },
                 new Invoice{ Id = id3 }
            };

            var query = receipts.AsQueryable().ApplyFilterItem(builder.Build());

            Assert.Equal(1, query.Count());
            Assert.Equal(id3, query.First().Id);

            builder.Operator = RelationalCompareOperator.IsNotNull;
            query = receipts.AsQueryable().ApplyFilterItem(builder.Build());

            Assert.Equal(2, query.Count());
            Assert.All(query, p => Assert.NotEqual(id3, p.Id));

            builder.Operator = RelationalCompareOperator.GreaterThen;
            builder.Right = new DateTimeConstantValueBuilder { Value = DateTime.Today };
            query = receipts.AsQueryable().ApplyFilterItem(builder.Build());

            Assert.Equal(1, query.Count());
            Assert.Equal(id2, query.First().Id);
        }

        [Fact]
        public void NumericFilterItemWithEnumTest()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();

            var receipt1 = new Invoice
            {
                Id = id1,
                Customer = new Contact { Id = id2, ContactType = ContactType.Customer }
            };

            var receipt2 = new Invoice
            {
                Id = id3,
                Customer = new Contact { Id = id4, ContactType = ContactType.Supplier }
            };

            var receipts = new[] {
                receipt1,
                receipt2
            };

            var filterItem = new NumericFilterItemBuilder
            {
                Left = new PathValueExpressionBuilder { Path = "Customer.ContactType" },
                Operator = RelationalCompareOperator.Equal,
                Right = new NumericConstantValueBuilder { Value = (int)ContactType.Customer }
            };

            var query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());
            Assert.Equal(receipt1, query.First());

            filterItem = new NumericFilterItemBuilder
            {
                Right = new PathValueExpressionBuilder { Path = "Customer.ContactType" },
                Operator = RelationalCompareOperator.Equal,
                Left = new NumericConstantValueBuilder { Value = (int)ContactType.Supplier }
            };

            query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(receipt2, query.First());
        }

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
        public void QueryModelBooleanFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"),BooleanOperator.IsTrue)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"),BooleanOperator.IsFalse)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelBuilderAlialsAndRootSourceMethodTest()
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

            var source = builder.Source(p => p.ReceiptDetail);

            Assert.NotNull(source);

            Assert.Throws<InvalidOperationException>(() => builder.Source());
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
        public void QueryModelBuilderRootAndAliasSourceMethodTest()
        {
            var builder = QueryModel.Build(p => p.Get<ReceiptDetail>(),
            p => new
            {
                Amount = p.Amount
            });

            var source = builder.Source();

            Assert.NotNull(source);

            Assert.Throws<InvalidOperationException>(() => builder.Source(p => p.Amount));
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
        public void QueryModelBuildSimpleAsync()
        {
            QueryModel model = GetSampleModel();

            Assert.NotNull(model);

            var selectItems = model.ResultType.GetProperties().OrderBy(p => p.Name).Skip(2).Select(p => new SelectItem(p.Name, Aggregate.None));

            var dataSource = new DummyQuerySource();
            var dummyReceipt = CreateDummyReceipt();

            dataSource.RegisterData(dummyReceipt.Details);

            var config = new QueryConfiguration(selectItems, Enumerable.Empty<SortItem>(), Enumerable.Empty<FilterItem>());
            var query = model.GetQuery(dataSource, config);

            var result = query.Cast<object>().ToList();

            Assert.NotEmpty(result);
        }

        [Fact]
        public void QueryModelDateTimeFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)),RelationalCompareOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)),RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-110)),RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-160)),RelationalCompareOperator.GreaterThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-160)),RelationalCompareOperator.GreaterThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)),RelationalCompareOperator.LessThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"),new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)),RelationalCompareOperator.LessThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelInvalidPathException()
        {
            Assert.Throws<InvalidPathException>(() => new PathValueExpression("Article.Nonexistant").GetExpression(Expression.Parameter(typeof(ReceiptDetail))));
        }

        [Fact]
        public void QueryModelNumericFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<int>(120),RelationalCompareOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<int>(90),RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<int>(91),RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<decimal>(90),RelationalCompareOperator.GreaterThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<decimal>(90),RelationalCompareOperator.GreaterThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<double>(120),RelationalCompareOperator.LessThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"),new ConstantValueExpression<float>(120),RelationalCompareOperator.LessThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelSingleSourceAsync()
        {
            var dataSource = new DummyQuerySource();
            var dummyReceipt = CreateDummyReceipt();
            dataSource.RegisterData(new[] { dummyReceipt.Customer });

            var builder = QueryModel.Build(
                p => p.Get<Contact>(),
                p => new ContactInfo
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Street = p.Street
                });

            var model = builder.ToModel();

            var queryConfig = new QueryConfiguration(
                new[] {
                    new SelectItem("Id",Aggregate.None),
                    new SelectItem("FirstName",Aggregate.None),
                    new SelectItem("LastName",Aggregate.None)
                });

            var query = model.GetQuery(dataSource, queryConfig).ToList();

            Assert.NotEmpty(query);

            Assert.All(query, p =>
            {
                Assert.Null(p.Street);
                Assert.NotNull(p.FirstName);
            });
        }

        [Fact]
        public void QueryModelSingleSourceWithFilterAsync()
        {
            var dataSource = new DummyQuerySource();
            var dummyReceipt = CreateDummyReceipt();
            dataSource.RegisterData(dummyReceipt.Details);

            var builder = QueryModel.Build(
                p => p.Get<ReceiptDetail>(),
                p => new
                {
                    Id = p.Id,
                    ArticleNumber = p.Article.ArticleNumber,
                    Description = p.Description,
                    Amount = p.Amount,
                });

            builder.Source().Query(p => p.Query<ReceiptDetail>().Where(x => x.Enabled));

            var model = builder.ToModel();

            var queryConfig = new QueryConfiguration(
                new[] {
                    new SelectItem("Id",Aggregate.None),
                    new SelectItem("ArticleNumber",Aggregate.None),
                    new SelectItem("Amount",Aggregate.None)
                });

            var query = model.GetQuery(dataSource, queryConfig).ToList();

            Assert.NotEmpty(query);

            Assert.Equal(1, query.Count);

            Assert.All(query, p =>
            {
                Assert.Null(p.Description);
                Assert.NotNull(p.ArticleNumber);
            });
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
        public void QueryModelStringFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("Testdescription"),StringOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("Testdescription"),StringOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("Testdescription2"),StringOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("TEST"),StringOperator.StartsWith)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("ION"),StringOperator.EndsWith)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"),new ConstantValueExpression<string>("descri"),StringOperator.Contains)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelWithSourceAndTragetFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt("Demo1");
            var receipt2 = CreateDummyReceipt("Demo2");
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details.Concat(receipt2.Details));

            FilterItem[] filterItems = {
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"),BooleanOperator.IsTrue)
            };
            FilterItem[] targetFilterItems = {
                new StringBinaryFilterItem(new PathValueExpression("Customer"),new StringConstantValue("Demo2"),StringOperator.Contains)
            };

            SortItem[] sortItems = {
                new SortItem("Supplier",SortDirection.Ascending)
            };
            SelectItem[] selectItems = {
                new SelectItem("ReceiptDetailId"  ),
                new SelectItem("ReceiptNumber"    ),
                new SelectItem("ArticleNumber"    ),
                new SelectItem("ArticleSoldYear"  ),
                new SelectItem("ArticleSoldMonth" )
            };

            var config = new QueryConfiguration(selectItems, sortItems, filterItems, targetFilterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());
            Assert.All(query.Cast<object>(), p => Assert.NotNull(p.GetType().GetProperty("Supplier").GetValue(p)));

            Assert.All(query.Cast<object>(), p => Assert.Contains("Demo2", (string)p.GetType().GetProperty("Customer").GetValue(p)));
        }

        [Fact]
        public void SourceEntityConfigurationConstructorTest()
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

        private static QueryModel GetSampleModel()
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
                            ReceiptAmountCurrentYear = year != null ? year.Sum(x => x.Amount) : 0,
                            ReceiptAmountCurrentMonth = currentMonth != null ? currentMonth.Sum(x => x.Amount) : 0,
                            ReceiptAmountLastMonth = lastMonth != null ? lastMonth.Sum(x => x.Amount) : 0
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
                            SoldCurrentYear = year != null ? year.Sum(x => x.Amount) : 0,
                            SoldCurrentMonth = currentMonth != null ? currentMonth.Sum(x => x.Amount) : 0,
                            SoldLastMonth = lastMonth != null ? lastMonth.Sum(x => x.Amount) : 0
                        }
                      )
                      .Join(p => p.ArticleId, p => p.ReceiptDetail.ArticleId);

            QueryModel model = builder.ToModel();
            return model;
        }

        private Receipt CreateDummyReceipt(string customerFirstName = "Demo")
        {
            var country = new Country { Id = 1, CountryName = "Austria" };

            var customer = new Contact
            {
                Id = Guid.NewGuid(),
                Country = country,
                CountryId = country.Id,
                FirstName = customerFirstName,
                LastName = "Customer",
                Identity = "democustomer",
                Street = "demostreet"
            };

            var receipt = new Invoice
            {
                Id = Guid.NewGuid(),
                Customer = customer,
                CustomerId = customer.Id,
                ReceiptDate = DateTime.Today.AddDays(-10),
                ReceiptNumber = "123456"
            };

            var supplier = new Contact
            {
                Id = Guid.NewGuid(),
                Country = country,
                CountryId = country.Id,
                FirstName = "Demo",
                LastName = "Supplier",
                Identity = "demosupplier",
                Street = "demostreet"
            };

            var article = new Article
            {
                Id = Guid.NewGuid(),
                ArticleDescription = "ArticleDemodDescription",
                ArticleNumber = "12345",
                Price = 12,
                Supplier = supplier,
                SupplierId = supplier.Id
            };

            var detail = new ReceiptDetail
            {
                Id = Guid.NewGuid(),
                Amount = 120.0m,
                Description = "Testdescription",
                Price = 12,
                Receipt = receipt,
                ReceiptId = receipt.Id,
                Article = article,
                ArticleId = article.Id,
                DeliveryTime = DateTime.Today.AddDays(-150),
                Enabled = true
            };
            receipt.Details.Add(detail);

            var detail2 = new ReceiptDetail
            {
                Id = Guid.NewGuid(),
                Amount = 90.0m,
                Description = "Testdescription2",
                Price = 12,
                Receipt = receipt,
                ReceiptId = receipt.Id,
                Article = article,
                ArticleId = article.Id,
                DeliveryTime = DateTime.Today.AddDays(-160),
                Enabled = false
            };

            receipt.Details.Add(detail2);

            return receipt;
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