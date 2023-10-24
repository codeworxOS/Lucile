using Lucile.Linq;
using Lucile.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.Core.Test
{
    public class DerivedTypesAndMappingsTest
    {
        [Fact]
        public void QueryModel_BuildWithDerivedTypesAndMappings_BuiltWithoutException()
        {
            var services = new ServiceCollection();

            services.AddMapper();
            services.AddMapping<OwnerEntity>()
                .Configure(b => b.To(e => new DisplayValue
                {
                    Id = e.Id,
                    Value = e.Name
                }))
                .Configure(b => b.Base<OwnerEntity, DisplayValue>().Extend(e => new ExtendedDisplayValue
                {
                    AdditionalInfo = e.Permissions
                }));
            services.AddMapping<NodeQuerySource>()
                .Configure(b => b.To(n => new Node
                {
                    Id = n.Node.Id,
                    Name = n.Node.Name,
                    Company = new DisplayValue
                    {
                        Id = n.Node.CompanyId,
                        Value = n.Node.CompanyName
                    },
                    Owner = n.Owner.Map<OwnerEntity, ExtendedDisplayValue>()
                }));

            var queryModel = QueryModel.Create(b => new NodeQuerySource { Node = b.Get<NodeEntity>(), Owner = b.Get<OwnerEntity>() }, n => n.Map<NodeQuerySource, Node>());

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var mapperFactory = serviceProvider.GetRequiredService<IMapperFactory>();
                var exception = Record.Exception(() => queryModel.Build(mapperFactory));

                Assert.Null(exception);
            }
        }

        private class NodeQuerySource
        {
            public NodeEntity Node { get; set; }
            public OwnerEntity Owner { get; set; }
        }

        private class NodeEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public OwnerEntity Owner { get; set; }
        }

        private class OwnerEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Permissions { get; set; }
        }

        private class Node
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DisplayValue Company { get; set; }
            public ExtendedDisplayValue Owner { get; set; }
        }

        private class DisplayValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        private class ExtendedDisplayValue : DisplayValue
        {
            public string AdditionalInfo { get; set; }
        }
    }
}