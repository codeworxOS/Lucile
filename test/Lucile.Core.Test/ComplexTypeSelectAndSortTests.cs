using System.Linq;
using Lucile.Linq;
using Lucile.Linq.Configuration;
using Lucile.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Tests;
using Xunit;

namespace Lucile.Core.Test
{
    public class ComplexTypeSelectAndSortTests
    {
        [Fact]
        public void QueryModel_QueryWithComplexTypeInSelectAndSort_ComplexPropertyLoaded()
        {
            const string expectedNodeName = "Test Node ZZZ";
            const int expectedCompanyId = 123;
            const string expectedCompanyName = "Test Company XXX";

            var services = new ServiceCollection();
            services.AddMapper();
            services.AddMapping<NodeQuerySource>()
                .Configure(b => b.To(n => new Node
                {
                    Id = n.Node.Id,
                    Name = n.Node.Name,
                    Company = new DisplayValue
                    {
                        Id = n.Node.CompanyId,
                        Value = n.Node.CompanyName,
                        SortOrder = n.Node.CompanySortOrder
                    }
                }));

            var queryConfiguration = new QueryConfiguration(
                selectItems: new[] { new SelectItem("Name")/*, new SelectItem("Company")*/ },
                sortItems: new[] { new SortItem("Company.SortOrder", SortDirection.Ascending) },
                filterItems: Enumerable.Empty<FilterItem>());

            var querySource = new DummyQuerySource();
            querySource.RegisterData(new[]
            {
                new NodeEntity { Id = 1, Name = expectedNodeName, CompanyId = expectedCompanyId, CompanyName = expectedCompanyName, CompanySortOrder = 1 }
            });

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var mapperFactory = serviceProvider.GetRequiredService<IMapperFactory>();
                var query = QueryModel
                    .Create(b => new NodeQuerySource { Node = b.Get<NodeEntity>() }, n => n.Map<NodeQuerySource, Node>())
                    .Build(mapperFactory)
                    .GetQuery(querySource, queryConfiguration);

                var result = query.ToList();

                Assert.NotEmpty(result);
                Assert.Equal(expectedNodeName, result[0].Name);
                Assert.Equal(expectedCompanyId, result[0].Company.Id);
                Assert.Equal(expectedCompanyName, result[0].Company.Value);
            }
        }

        private class NodeQuerySource
        {
            public NodeEntity Node { get; set; }
        }

        private class NodeEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public int CompanySortOrder { get; set; }
        }

        private class Node
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DisplayValue Company { get; set; }
        }

        private class DisplayValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public int SortOrder { get; set; }
        }
    }
}