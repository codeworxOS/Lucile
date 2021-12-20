using System.Collections.Generic;
using System.Linq;
using Lucile.Linq.Builder;
using Lucile.Linq.Configuration;
using Lucile.Linq.Configuration.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class LinqExtensionsTest
    {
        [Fact]
        public void ApplyBooleanFilterItem_NullableBoolean()
        {
            var items = new List<ReceiptDetail>
            {
                new ReceiptDetail { Disabled = true, },
                new ReceiptDetail { Disabled = null, },
                new ReceiptDetail { Disabled = false, }
            };

            var query = items.AsQueryable();

            var builder = new BooleanFilterItemBuilder
            {
                Value = new PathValueExpressionBuilder { Path = "Disabled" },
                Operator = BooleanOperator.IsTrue
            };


            var configBuilder = new QueryConfigurationBuilder();
            configBuilder.FilterItems.Add(builder);

            var result = query.Apply(configBuilder.Build());

            Assert.Single(result);
        }


        [Fact]
        public void ApplyBooleanFilterItem_NullableBoolean_Not()
        {
            var items = new List<ReceiptDetail>
            {
                new ReceiptDetail { Disabled = true, },
                new ReceiptDetail { Disabled = null, },
                new ReceiptDetail { Disabled = false, }
            };

            var query = items.AsQueryable();

            var builder = new BooleanFilterItemBuilder
            {
                Value = new PathValueExpressionBuilder { Path = "Disabled" },
                Operator = BooleanOperator.IsFalse
            };


            var configBuilder = new QueryConfigurationBuilder();
            configBuilder.FilterItems.Add(builder);

            var result = query.Apply(configBuilder.Build());

            Assert.Single(result);
        }

        [Fact]
        public void ApplyConfig()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var builder = new StringFilterItemBuilder();
            builder.Left = new PathValueExpressionBuilder { Path = "LastName" };
            builder.Right = new StringConstantValueBuilder { Value = "Doe" };
            builder.Operator = Lucile.Linq.Configuration.StringOperator.Equal;

            var configBuilder = new QueryConfigurationBuilder();
            configBuilder.FilterItems.Add(builder);
            configBuilder.SortItems.Add(new SortItemBuilder() { PropertyPath = "FirstName", SortDirection = SortDirection.Ascending });

            var result = query.Apply(configBuilder.Build());

            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal("Doe", p.LastName));
            Assert.Equal(items[2], result.ToList()[0]);
            Assert.Equal(items[0], result.ToList()[1]);
        }

        [Fact]
        public void ApplySimpleStringFilterItem()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var builder = new StringFilterItemBuilder();
            builder.Left = new PathValueExpressionBuilder { Path = "LastName" };
            builder.Right = new StringConstantValueBuilder { Value = "Doe" };
            builder.Operator = Lucile.Linq.Configuration.StringOperator.Equal;

            var item = builder.Build();

            var result = query.ApplyFilterItem(item);

            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal("Doe", p.LastName));
        }

        [Fact]
        public void ApplySortAscAsc()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var sort = new[] {
                new SortItem("LastName", SortDirection.Ascending),
                new SortItem("FirstName", SortDirection.Ascending)
            };

            var result = query.ApplySort(sort).ToList();

            Assert.Equal(items[2], result[0]);
            Assert.Equal(items[1], result[1]);
            Assert.Equal(items[0], result[2]);
        }

        [Fact]
        public void ApplySortAscDesc()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var sort = new[] {
                new SortItem("LastName", SortDirection.Ascending),
                new SortItem("FirstName", SortDirection.Descending)
            };

            var result = query.ApplySort(sort).ToList();

            Assert.Equal(items[1], result[0]);
            Assert.Equal(items[2], result[1]);
            Assert.Equal(items[0], result[2]);
        }

        [Fact]
        public void ApplySortDescAsc()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var sort = new[] {
                new SortItem("LastName", SortDirection.Descending),
                new SortItem("FirstName", SortDirection.Ascending)
            };

            var result = query.ApplySort(sort).ToList();

            Assert.Equal(items[0], result[0]);
            Assert.Equal(items[2], result[1]);
            Assert.Equal(items[1], result[2]);
        }

        [Fact]
        public void ApplySortDescDesc()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Max", LastName = "Mustermann" },
                new Contact { FirstName = "John", LastName = "Doe" },
                new Contact { FirstName = "Jane", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var sort = new[] {
                new SortItem("LastName", SortDirection.Descending),
                new SortItem("FirstName", SortDirection.Descending)
            };

            var result = query.ApplySort(sort).ToList();

            Assert.Equal(items[0], result[0]);
            Assert.Equal(items[1], result[1]);
            Assert.Equal(items[2], result[2]);
        }

        [Fact]
        public void ApplySortPropertyPath()
        {
            var items = new List<Contact> {
                new Contact { FirstName = "Bla", LastName = "Mustermann" },
                new Contact { FirstName = "BlaBla", LastName = "Doe" },
                new Contact { FirstName = "BlaBlaBla", LastName = "Doe" }
            };

            var query = items.AsQueryable();

            var sort = new[] {
                new SortItem("FirstName.Length", SortDirection.Descending)
            };

            var result = query.ApplySort(sort).ToList();

            Assert.Equal(items[2], result[0]);
            Assert.Equal(items[1], result[1]);
            Assert.Equal(items[0], result[2]);
        }
    }
}