using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using Linq.Configuration;
using Lucile.Data.Metadata;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Lucile.Linq.Configuration.Builder;
using Lucile.Mapper;
using Lucile.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Xunit;
using Xunit.Sdk;

namespace Tests
{
    public class QueryModelTest
    {
        [Fact]
        public void AnyFilterItemNewtonsoftSerializationTest()
        {
            var value =
@"{
    ""type"": ""AnyFilterItemBuilder"",
    ""path"": ""Details"",
    ""operator"": null,
}";
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();


            var builder = Newtonsoft.Json.JsonConvert.DeserializeObject<AnyFilterItemBuilder>(value);

            Assert.Equal("Details", builder.Path);
            Assert.Null(builder.Filter);
            Assert.Null(builder.Operator);
        }

        [Fact]
        public void AnyFilterItemProtobufSerializationTest()
        {
            var value = new AnyFilterItemBuilder
            {
                Path = "Details",
            };

            AnyFilterItemBuilder newValue;

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, value);
                ms.Seek(0, SeekOrigin.Begin);

                newValue = ProtoBuf.Serializer.Deserialize<AnyFilterItemBuilder>(ms);
            }

            Assert.Equal("Details", newValue.Path);
            Assert.Null(newValue.Filter);
            Assert.Null(newValue.Operator);
        }


        [Fact]
        public void PathConvertSyntax_ExpectsOK()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var date = DateTime.Now;

            var receipt1 = new Invoice
            {
                Id = id1,
                ExpectedDeliveryDate = date,
                ReceiptType = ReceiptType.Invoice,
            };

            receipt1.Details.Add(new ReceiptDetail { Price = 101, Receipt = receipt1 });

            var receipt2 = new Invoice
            {
                Id = id2,
                ReceiptType = ReceiptType.Invoice,
            };

            receipt2.Details.Add(new ReceiptDetail { Price = 100, Receipt = receipt2 });

            var receipt3 = new Order
            {
                Id = id2,
                ReceiptType = ReceiptType.Offer,
            };

            receipt3.Details.Add(new ReceiptDetail { Price = 50, Receipt = receipt3 });


            var receipts = new List<Receipt>() {
                receipt1,
                receipt2,
                receipt3
            };

            var details = receipts.SelectMany(p => p.Details);

            var filterItem =
                new FilterItemGroupBuilder
                {
                    GroupType = Linq.Configuration.GroupType.And,
                    Children = {
                        new NumericFilterItemBuilder
                        {
                            Left = new PathValueExpressionBuilder{ Path = "Receipt.ReceiptType"},
                            Operator = RelationalCompareOperator.Equal,
                            Right = new NumericConstantValueBuilder { Value = 0},
                        },
                        new DateTimeFilterItemBuilder()
                        {

                            Left = new PathValueExpressionBuilder { Path = "Receipt.<Invoice>.ExpectedDeliveryDate" },
                            Operator = RelationalCompareOperator.Equal,
                            Right = new DateTimeConstantValueBuilder { Value = date },

                        },
                    }
                };

            var query = details.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());
            Assert.Equal(receipt1.Details[0], query.First());
        }

        [Fact]
        public void PathConvertSyntaxRoot_ExpectsOK()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var date = DateTime.Now;

            var receipt1 = new Invoice
            {
                Id = id1,
                ExpectedDeliveryDate = date,
                ReceiptType = ReceiptType.Invoice,
            };

            receipt1.Details.Add(new ReceiptDetail { Price = 101, Receipt = receipt1 });

            var receipt2 = new Invoice
            {
                Id = id2,
                ReceiptType = ReceiptType.Invoice,
            };

            receipt2.Details.Add(new ReceiptDetail { Price = 100, Receipt = receipt2 });

            var receipt3 = new Order
            {
                Id = id2,
                ReceiptType = ReceiptType.Offer,
            };

            receipt3.Details.Add(new ReceiptDetail { Price = 50, Receipt = receipt3 });


            var receipts = new List<Receipt>() {
                receipt1,
                receipt2,
                receipt3
            };


            var filterItem =
                new FilterItemGroupBuilder
                {
                    GroupType = Linq.Configuration.GroupType.And,
                    Children = {
                        new NumericFilterItemBuilder
                        {
                            Left = new PathValueExpressionBuilder{ Path = "ReceiptType"},
                            Operator = RelationalCompareOperator.Equal,
                            Right = new NumericConstantValueBuilder { Value = 0},
                        },
                        new DateTimeFilterItemBuilder()
                        {

                            Left = new PathValueExpressionBuilder { Path = "<Invoice>.ExpectedDeliveryDate" },
                            Operator = RelationalCompareOperator.Equal,
                            Right = new DateTimeConstantValueBuilder { Value = date },

                        },
                    }
                };

            var query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());
            Assert.Equal(receipt1, query.First());
        }

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
        public void NotAnyFilterItemTest()
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
                Operator = AnyOperator.NotAny,
                Path = "Details",
            };

            var query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(id1, query.First().Id);

            filterItem.Operator = AnyOperator.Any;

            query = receipts.AsQueryable().ApplyFilterItem(filterItem.Build());
            Assert.Equal(1, query.Count());

            Assert.Equal(id2, query.First().Id);
        }

        [Fact]
        public void GeneratedQueryWithComplexModelEqualityTest()
        {
            var expectedQueryModel = GetSampleModel();

            var expectedString = expectedQueryModel.GetQuery(new DummyQuerySource(), new QueryConfiguration()).Expression.ToString();
            for (int i = 0; i < 10; i++)
            {
                var actualQueryModel = GetSampleModel();
                var actualString = actualQueryModel.GetQuery(new DummyQuerySource(), new QueryConfiguration()).Expression.ToString();

                Assert.Equal(expectedString, actualString);
            }
        }

        [Fact]
        public void GeneratedQueryWithFiltersEqualityTest()
        {
            var queryModel = QueryModel.Create(
                builder => builder.Get<ReceiptDetail>(),
                builder => new ReceiptDetail
                {
                    ReceiptId = builder.ReceiptId,
                    ArticleId = builder.ArticleId,
                })
                .HasKey(receipt => receipt.ReceiptId)
                .Build();

            FilterItem[] filterItems = {
                new GuidBinaryFilterItem(new PathValueExpression(nameof(ReceiptDetail.ReceiptId)), new ConstantValueExpression<Guid>(Guid.NewGuid()), RelationalCompareOperator.Equal)
            };
            var queryConfiguration = new QueryConfiguration(filterItems);
            var query1 = queryModel.GetQuery(new DummyQuerySource(), queryConfiguration);
            var query1String = query1.Expression.ToString();

            filterItems = new FilterItem[] {
                new GuidBinaryFilterItem(new PathValueExpression(nameof(ReceiptDetail.ReceiptId)), new ConstantValueExpression<Guid>(Guid.NewGuid()), RelationalCompareOperator.Equal)
            };
            queryConfiguration = new QueryConfiguration(filterItems);
            var query2 = queryModel.GetQuery(new DummyQuerySource(), queryConfiguration);
            var query2String = query2.Expression.ToString();

            var areEqual = query1String == query2String;
            Assert.True(areEqual);
        }

        [Fact]
        public void GeneratedQueryWithSimpleModelEqualityTest()
        {
            var expectedQueryModel = QueryModel.Create(
                builder => builder.Get<ReceiptDetail>(),
                builder => new ReceiptDetail
                {
                    ReceiptId = builder.ReceiptId,
                    ArticleId = builder.ArticleId,
                })
                .HasKey(receipt => receipt.ReceiptId)
                .Build();
            var expectedString = expectedQueryModel.GetQuery(new DummyQuerySource(), new QueryConfiguration()).Expression.ToString();

            for (int i = 0; i < 10; i++)
            {
                var actualQueryModel = QueryModel.Create(
                    builder => builder.Get<ReceiptDetail>(),
                    builder => new ReceiptDetail
                    {
                        ReceiptId = builder.ReceiptId,
                        ArticleId = builder.ArticleId,
                    })
                    .HasKey(receipt => receipt.ReceiptId)
                    .Build();

                var actualString = actualQueryModel.GetQuery(new DummyQuerySource(), new QueryConfiguration()).Expression.ToString();
                Assert.Equal(expectedString, actualString);
            }
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

            Assert.Throws<ArgumentException>(() => new PropertyConfigurationBuilder<ReceiptDetail, string>(p => p.Amount.ToString()));
            Assert.Throws<ArgumentException>(() => new PropertyConfigurationBuilder<ReceiptDetail, int>(p => p.Amount.ToString().Length));
        }

        [Fact]
        public void PropertyConfigurationDependenciesTest()
        {
            var builder = QueryModel.Create(
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

            var model = builder.Build();

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
        public void QueryModelMultipleFiltersForSameProperty()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new StringBinaryFilterItem(new PathValueExpression("ArticleNumber"), new ConstantValueExpression<string>("12345"), StringOperator.Equal),
                new StringBinaryFilterItem(new PathValueExpression("ArticleNumber"), new ConstantValueExpression<string>("notexisting"), StringOperator.Equal)
            };
            FilterItem[] filterItemGroup =
            {
                new FilterItemGroup(filterItems, GroupType.Or)
            };
            SelectItem[] selectItems =
            {
                new SelectItem("ReceiptNumber")
            };
            var config = new QueryConfiguration(selectItems, Enumerable.Empty<SortItem>(), Enumerable.Empty<FilterItem>(), filterItemGroup);

            var query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelBooleanFilterAsync()
        {
            var model = GetSampleModel();

            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            FilterItem[] filterItems = {
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"), BooleanOperator.IsTrue)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"), BooleanOperator.IsFalse)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelBuilderAlialsAndRootSourceMethodTest()
        {
            var builder = QueryModel.Create(p =>
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
            var builder1 = QueryModel.Create(p => p.Get<ReceiptDetail>(), p => new { Amount = p.Amount });
            Assert.True(builder1.IsSingleSourceQuery);

            Assert.Throws<ArgumentException>(() => QueryModel.Create(p => default(ReceiptDetail), p => new { Amount = p.Amount }));

            var builder2 = QueryModel.Create(p => new { ReceiptDetail = p.Get<ReceiptDetail>() }, p => new { Amount = p.ReceiptDetail.Amount });
            Assert.False(builder2.IsSingleSourceQuery);

            Assert.Throws<ArgumentException>(() => QueryModel.Create(p => new { ReceiptDetail = new ReceiptDetail() }, p => new { Amount = p.ReceiptDetail.Amount }));

            var builder3 = QueryModel.Create(p => new SourceClass { ReceiptDetail = p.Get<ReceiptDetail>() }, p => new AmountResult { Amount = p.ReceiptDetail.Amount });
            Assert.False(builder3.IsSingleSourceQuery);
        }

        [Fact]
        public void QueryModelBuilderCreateCompoundTypeInSelect()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var builder = QueryModel.Create(
                p => p.Get<ReceiptDetail>(),
                p => new
                {
                    p.Id,
                    Article = new IdentifierValue
                    {
                        Id = p.ArticleId,
                        Name = p.Article.ArticleNumber
                    }
                });
            var model = builder.Build();

            var targetFilterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("Article.Name"), new ConstantValueExpression<string>("123"), StringOperator.Contains)
            };
            var queryConfiguration = new QueryConfiguration(
                Enumerable.Empty<SelectItem>(),
                Enumerable.Empty<SortItem>(),
                Enumerable.Empty<FilterItem>(),
                targetFilterItems);

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void QueryModelBuilderCreateCompoundTypeEnumerableInSelect()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var builder = QueryModel.Create(
                p => p.Get<Receipt>(),
                p => new
                {
                    p.Id,
                    p.ReceiptNumber,
                    Details = p.Details.Select(x => new { Id = x.Id, Article = x.Article.ArticleNumber })
                });
            var model = builder.Build();

            var details = model.Entity.GetNavigations().Where(p => p.Name == "Details").FirstOrDefault();

            Assert.NotNull(details);
            Assert.NotNull(details.TargetEntity);
            Assert.Null(details.TargetNavigationProperty);
            Assert.Equal(NavigationPropertyMultiplicity.Many, details.Multiplicity);

            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Id");
            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Article");
        }


        [Fact]
        public void QueryModelBuilderCreateCompoundTypeListInSelect()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var builder = QueryModel.Create(
                p => p.Get<Receipt>(),
                p => new
                {
                    p.Id,
                    p.ReceiptNumber,
                    Details = p.Details.Select(x => new { Id = x.Id, Article = x.Article.ArticleNumber }).ToList(),
                });
            var model = builder.Build();

            var details = model.Entity.GetNavigations().Where(p => p.Name == "Details").FirstOrDefault();

            Assert.NotNull(details);
            Assert.NotNull(details.TargetEntity);
            Assert.Null(details.TargetNavigationProperty);
            Assert.Equal(NavigationPropertyMultiplicity.Many, details.Multiplicity);

            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Id");
            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Article");
        }



        [Fact]
        public void QueryModelBuilderCreateCompoundTypeListSelectProperty()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(new[] { receipt });

            var builder = QueryModel.Create(
                p => p.Get<Receipt>(),
                p => new
                {
                    p.Id,
                    p.ReceiptNumber,
                    Details = p.Details.Select(x => new { Id = x.Id, Article = x.Article.ArticleNumber }).ToList(),
                });
            var model = builder.Build();

            var details = model.Entity.GetNavigations().Where(p => p.Name == "Details").FirstOrDefault();

            Assert.NotNull(details);
            Assert.NotNull(details.TargetEntity);
            Assert.Null(details.TargetNavigationProperty);
            Assert.Equal(NavigationPropertyMultiplicity.Many, details.Multiplicity);

            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Id");
            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Article");

            var queryConfiguration = new QueryConfiguration(
                new[] { new SelectItem("Id"), new SelectItem("Details") },
                Enumerable.Empty<SortItem>(),
                Enumerable.Empty<FilterItem>(),
                Enumerable.Empty<FilterItem>());

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();

            Assert.True(result.Any());
            Assert.All(result, p => Assert.Null(p.ReceiptNumber));
            Assert.All(result, p => Assert.NotEqual(default(Guid), p.Id));
            Assert.All(result, p => Assert.True(p.Details.Any()));
        }

        [Fact]
        public void QueryModelBuilderCreateCompoundTypeListExcludeProperty()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(new[] { receipt });

            var builder = QueryModel.Create(
                p => p.Get<Receipt>(),
                p => new
                {
                    p.Id,
                    p.ReceiptNumber,
                    Details = p.Details.Select(x => new { Id = x.Id, Article = x.Article.ArticleNumber }).ToList(),
                });
            var model = builder.Build();

            var details = model.Entity.GetNavigations().Where(p => p.Name == "Details").FirstOrDefault();

            Assert.NotNull(details);
            Assert.NotNull(details.TargetEntity);
            Assert.Null(details.TargetNavigationProperty);
            Assert.Equal(NavigationPropertyMultiplicity.Many, details.Multiplicity);

            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Id");
            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Article");

            var queryConfiguration = new QueryConfiguration(
                new[] { new SelectItem("Id"), new SelectItem("ReceiptNumber") },
                Enumerable.Empty<SortItem>(),
                Enumerable.Empty<FilterItem>(),
                Enumerable.Empty<FilterItem>());

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();

            Assert.True(result.Any());
            Assert.All(result, p => Assert.NotNull(p.ReceiptNumber));
            Assert.All(result, p => Assert.NotEqual(default(Guid), p.Id));
            Assert.All(result, p => Assert.Null(p.Details));
        }

        [Fact]
        public void QueryModelBuilderCreateCompoundTypeListTargetFilter()
        {
            var receipt = CreateDummyReceipt();
            var receipt2 = CreateDummyReceipt();
            receipt2.Details.Clear();

            var source = new DummyQuerySource();
            source.RegisterData(new[] { receipt, receipt2 });

            var builder = QueryModel.Create(
                p => p.Get<Receipt>(),
                p => new
                {
                    p.Id,
                    p.ReceiptNumber,
                    Details = p.Details.Select(x => new { Id = x.Id, Article = x.Article.ArticleNumber }).ToList(),
                });
            var model = builder.Build();

            var details = model.Entity.GetNavigations().Where(p => p.Name == "Details").FirstOrDefault();

            Assert.NotNull(details);
            Assert.NotNull(details.TargetEntity);
            Assert.Null(details.TargetNavigationProperty);
            Assert.Equal(NavigationPropertyMultiplicity.Many, details.Multiplicity);

            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Id");
            Assert.Contains(details.TargetEntity.GetProperties(), p => p.Name == "Article");

            var queryConfiguration = new QueryConfiguration(
                Enumerable.Empty<SelectItem>(),
                Enumerable.Empty<SortItem>(),
                Enumerable.Empty<FilterItem>(),
                new FilterItem[] { new AnyFilterItem(new PathValueExpression("Details"), null) });

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();

            Assert.Single(result);
        }

        [Fact]
        public void QueryModelBuilderSelectAnonymousTypeWithExcludedValueTypesInSelect()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var builder = QueryModel.Create(
                p => p.Get<ReceiptDetail>(),
                p => new
                {
                    p.Id,
                    p.DeliveryTime,
                    p.Description
                });
            var model = builder.Build();

            var queryConfiguration = new QueryConfiguration(
                new[] {
                    new SelectItem("Id"),
                    new SelectItem("Description"),
                });

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();
            Assert.NotEmpty(result);
            Assert.Equal(3, result.Count);
        }



        [Fact]
        public void QueryModelBuilderCreateDynamicCompoundTypeInSelect()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var builder = QueryModel.Create(
                p => p.Get<ReceiptDetail>(),
                p => new
                {
                    p.Id,
                    Article = new
                    {
                        p.ArticleId,
                        Number = p.Article.ArticleNumber
                    }
                });
            var model = builder.Build();

            var targetFilterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("Article.Number"), new ConstantValueExpression<string>("123"), StringOperator.Contains)
            };
            var queryConfiguration = new QueryConfiguration(
                Enumerable.Empty<SelectItem>(),
                Enumerable.Empty<SortItem>(),
                Enumerable.Empty<FilterItem>(),
                targetFilterItems);

            var query = model.GetQuery(source, queryConfiguration);
            var result = query.ToList();
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void QueryModelBuilderNonGenericSourceMethodTest()
        {
            var builder = QueryModel.Create(p =>
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
            var builder = QueryModel.Create(p => p.Get<ReceiptDetail>(),
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
            var builder = QueryModel.Create(
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
            var builder = QueryModel.Create(
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
            var builder = QueryModel.Create(
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
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)), RelationalCompareOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)), RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-110)), RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(3, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-160)), RelationalCompareOperator.GreaterThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-160)), RelationalCompareOperator.GreaterThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)), RelationalCompareOperator.LessThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new DateTimeBinaryFilterItem(new PathValueExpression("ReceiptDetail.DeliveryTime"), new ConstantValueExpression<DateTime>(DateTime.Today.AddDays(-150)), RelationalCompareOperator.LessThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(3, query.Cast<object>().Count());
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
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<int>(120), RelationalCompareOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<int>(90), RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<int>(91), RelationalCompareOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(3, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<decimal>(90), RelationalCompareOperator.GreaterThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<decimal>(90), RelationalCompareOperator.GreaterThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(3, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<double>(120), RelationalCompareOperator.LessThen)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new NumericBinaryFilterItem(new PathValueExpression("ReceiptDetail.Amount"), new ConstantValueExpression<float>(120), RelationalCompareOperator.LessThenOrEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(3, query.Cast<object>().Count());
        }

        [Fact]
        public void QueryModelResultWithBaseClass()
        {
            var model = QueryModel.Create(p => p.Get<ReceiptDetail>(),
                p => new ResultDto
                {
                    Id = p.Id,
                    Name = p.Description,
                    Amount = p.Amount
                }).Build();

            var query = model.GetQuery(new DummyQuerySource(), new QueryConfiguration());

            Assert.NotNull(query);
        }


        [Fact]
        public void QueryModelSelectWithCascadedPathAsync()
        {
            var dataSource = new DummyQuerySource();
            var dummyReceipt = CreateDummyReceipt();
            dataSource.RegisterData(new[] { dummyReceipt.Customer });

            var builder = QueryModel.Create(
                p => p.Get<Contact>(),
                p => new ContactListItem
                {

                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Street = p.Street,
                    Country = new Lucile.Core.Test.Model.Target.CountryInfo
                    {
                        Id = p.Country.Id,
                        DisplayText = p.Country.CountryName,
                    }
                });

            var model = builder.Build();

            var queryConfig = new QueryConfiguration(
                new[] {
                    new SelectItem("Id", Aggregate.None),
                    new SelectItem("Country.Id", Aggregate.None),
                    new SelectItem("LastName", Aggregate.None)
                });

            var query = model.GetQuery(dataSource, queryConfig).ToList();

            Assert.NotEmpty(query);

            Assert.All(query, p =>
            {
                Assert.Null(p.Street);
                Assert.Null(p.FirstName);
                Assert.NotNull(p.LastName);
                Assert.NotEqual(default, p.Country.Id);
                Assert.Null(p.Country.DisplayText);
            });
        }

        [Fact]
        public void QueryModelSingleSourceAsync()
        {
            var dataSource = new DummyQuerySource();
            var dummyReceipt = CreateDummyReceipt();
            dataSource.RegisterData(new[] { dummyReceipt.Customer });

            var builder = QueryModel.Create(
                p => p.Get<Contact>(),
                p => new ContactInfo
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Street = p.Street
                });

            var model = builder.Build();

            var queryConfig = new QueryConfiguration(
                new[] {
                    new SelectItem("Id", Aggregate.None),
                    new SelectItem("FirstName", Aggregate.None),
                    new SelectItem("LastName", Aggregate.None)
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

            var builder = QueryModel.Create(
                p => p.Get<ReceiptDetail>(),
                p => new
                {
                    Id = p.Id,
                    ArticleNumber = p.Article.ArticleNumber,
                    Description = p.Description,
                    Amount = p.Amount,
                });

            builder.Source().Query(p => p.Query<ReceiptDetail>().Where(x => x.Enabled));

            var model = builder.Build();

            var queryConfig = new QueryConfiguration(
                new[] {
                    new SelectItem("Id", Aggregate.None),
                    new SelectItem("ArticleNumber", Aggregate.None),
                    new SelectItem("Amount", Aggregate.None)
                });

            var query = model.GetQuery(dataSource, queryConfig).ToList();

            Assert.NotEmpty(query);

            Assert.Single(query);

            Assert.All(query, p =>
            {
                Assert.Null(p.Description);
                Assert.NotNull(p.ArticleNumber);
            });
        }

        [Fact]
        public void QueryModelMapperTest()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var services = new ServiceCollection();

            services
                .AddMapper()
                    .AddMapping<ReceiptDetail>()
                        .Configure(builder => builder.To(p => new ReceiptDetailInfo
                        {
                            Id = p.Id,
                            DisplayText = p.Article.CurrentName,
                        }));


            var builder = QueryModel.Create(
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
                            DetailInfo = p.ReceiptDetail.Map<ReceiptDetail, ReceiptDetailInfo>(),
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

            using (var sp = services.BuildServiceProvider())
            {
                var factory = sp.GetRequiredService<IMapperFactory>();

                var model = builder.Build(factory);
                var receiptDetail = model.SourceEntityConfigurations.First(p => p.Name == "ReceiptDetail");
                var articleStatistics = model.SourceEntityConfigurations.First(p => p.Name == "ArticleStatistics");
                var customerStatistics = model.SourceEntityConfigurations.First(p => p.Name == "CustomerStatistics");
                var whatever = model.SourceEntityConfigurations.First(p => p.Name == "Whatever");

                Assert.Equal(receiptDetail.DependsOn, new string[] { });
                Assert.Equal(articleStatistics.DependsOn, new string[] { "ReceiptDetail" });
                Assert.Equal(customerStatistics.DependsOn, new string[] { "ReceiptDetail" });
                Assert.Equal(whatever.DependsOn.OrderBy(p => p), new string[] { "ArticleStatistics", "CustomerStatistics" });

                var result = model.GetQuery(source, new QueryConfiguration()).ToList();

                Assert.All(result, p => Assert.NotNull(p.DetailInfo));
            }
        }

        [Fact]
        public void QueryModelMapperOnRootTest()
        {
            var receipt = CreateDummyReceipt();
            var source = new DummyQuerySource();
            source.RegisterData(receipt.Details);

            var services = new ServiceCollection();

            services
                .AddMapper()
                    .AddMapping<ReceiptDetail>()
                        .Configure(builder => builder.To(p => new ReceiptDetailInfo
                        {
                            Id = p.Id,
                            DisplayText = p.Article.CurrentName,
                        }));


            var builder = QueryModel.Create(
                        p => new
                        {
                            ReceiptDetail = p.Get<ReceiptDetail>(),
                            CustomerStatistics = p.Get<CustomerStatistics>(),
                            ArticleStatistics = p.Get<ArticleStatistics>(),
                            Whatever = p.Get<Whatever>()
                        },
                        p => p.ReceiptDetail.Map<ReceiptDetail, ReceiptDetailInfo>());

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

            using (var sp = services.BuildServiceProvider())
            {
                var factory = sp.GetRequiredService<IMapperFactory>();

                var model = builder.Build(factory);
                var receiptDetail = model.SourceEntityConfigurations.First(p => p.Name == "ReceiptDetail");
                var articleStatistics = model.SourceEntityConfigurations.First(p => p.Name == "ArticleStatistics");
                var customerStatistics = model.SourceEntityConfigurations.First(p => p.Name == "CustomerStatistics");
                var whatever = model.SourceEntityConfigurations.First(p => p.Name == "Whatever");

                Assert.Equal(receiptDetail.DependsOn, new string[] { });
                Assert.Equal(articleStatistics.DependsOn, new string[] { "ReceiptDetail" });
                Assert.Equal(customerStatistics.DependsOn, new string[] { "ReceiptDetail" });
                Assert.Equal(whatever.DependsOn.OrderBy(p => p), new string[] { "ArticleStatistics", "CustomerStatistics" });

                var result = model.GetQuery(source, new QueryConfiguration()).ToList();

                Assert.True(result.Any());
                Assert.All(result, p => Assert.NotNull(p.DisplayText));
            }
        }

        [Fact]
        public void QueryModelSourceDependenciesTest()
        {
            var builder = QueryModel.Create(
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

            var model = builder.Build();
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
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("Testdescription"), StringOperator.Equal)
            };
            var config = new QueryConfiguration(filterItems);

            var query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("Testdescription"), StringOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("Testdescription2"), StringOperator.NotEqual)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("TEST"), StringOperator.StartsWith)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(2, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("ION"), StringOperator.EndsWith)
            };
            config = new QueryConfiguration(filterItems);

            query = model.GetQuery(source, config);
            Assert.Equal(1, query.Cast<object>().Count());

            filterItems = new[] {
                new StringBinaryFilterItem(new PathValueExpression("ReceiptDetail.Description"), new ConstantValueExpression<string>("descri"), StringOperator.Contains)
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
                new BooleanFilterItem(new PathValueExpression("ReceiptDetail.Enabled"), BooleanOperator.IsTrue)
            };
            FilterItem[] targetFilterItems = {
                new StringBinaryFilterItem(new PathValueExpression("Customer"), new StringConstantValue("Demo2"), StringOperator.Contains)
            };

            SortItem[] sortItems = {
                new SortItem("Supplier", SortDirection.Ascending)
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
            var builder = QueryModel.Create(
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
            Assert.Single(toTest2.DependsOn);

            Assert.False(toTest3.IsRootQuery);
            Assert.Contains(toTest3.DependsOn, p => p == "ReceiptDetail");
            Assert.Single(toTest3.DependsOn);
        }

        private static QueryModel GetSampleModel()
        {
            var builder = QueryModel.Create(
                            p => new
                            {
                                ReceiptDetail = p.Get<ReceiptDetail>(),
                                CustomerStatistics = p.Get<CustomerStatistics>(),
                                ArticleStatistics = p.Get<ArticleStatistics>()
                            },
                            p => new SampleData
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
                        })
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

            QueryModel model = builder.Build();
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

            var article2 = new Article
            {
                Id = Guid.NewGuid(),
                ArticleDescription = "Whatever",
                ArticleNumber = "6789",
                Price = 120,
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

            var detail3 = new ReceiptDetail
            {
                Id = Guid.NewGuid(),
                Amount = 90.0m,
                Description = "whatever2",
                Price = 120,
                Receipt = receipt,
                ReceiptId = receipt.Id,
                Article = article2,
                ArticleId = article2.Id,
                DeliveryTime = DateTime.Today.AddDays(-170),
                Enabled = false
            };

            receipt.Details.Add(detail3);

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

        private class SampleData
        {
            public Guid ReceiptDetailId { get; set; }
            public string ReceiptNumber { get; set; }
            public string ArticleNumber { get; set; }
            public decimal ArticleSoldYear { get; set; }
            public decimal ArticleSoldMonth { get; set; }
            public decimal ArticleSoldLastMonth { get; set; }
            public decimal ArticleRevenueYear { get; set; }
            public decimal ArticleRevenueMonth { get; set; }
            public decimal ArticleRevenueLastMonth { get; set; }
            public string Supplier { get; set; }
            public string Customer { get; set; }
            public decimal CustomerRevenueYear { get; set; }
            public decimal CustomerRevenueMonth { get; set; }
            public decimal CustomerRevenueLastMonth { get; set; }
            public DateTime CustomerLastPurchase { get; set; }
        }

        private class ReceiptDetailInfo
        {
            public Guid Id { get; set; }
            public string DisplayText { get; set; }
        }
    }
}