using System;
using System.Collections.Generic;
using System.Linq;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Linq.Configuration.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Tests
{
    public class LinqExtensionsTest
    {
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

            var item = builder.ToTarget();

            var result = query.ApplyFilterItem(item);

            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal("Doe", p.LastName));
        }
    }
}