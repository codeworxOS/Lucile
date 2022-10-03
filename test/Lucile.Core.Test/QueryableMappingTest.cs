using System;
using System.Linq;
using Lucile.Core.Test.Model.Source;
using Lucile.Core.Test.Model.Target;
using Lucile.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.Core.Test
{
    public class QueryableMappingTest
    {
        [Fact]
        public void SourceTargetMapping_Base_Extend_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                .AddMapping<Person>()
                .Configure(builder => builder.To(p => new PersonInfo
                {
                    Id = p.Id,
                    DisplayName = p.FirstName + " " + p.LastName,
                }));

            services.AddMapping<Customer>()
                .Configure(builder => builder.Base<Person, PersonInfo>().Extend(c => new CustomerInfo
                {
                    Contact = c.Contact
                }));

            using (var sp = services.BuildServiceProvider())
            {
                var mapper = sp.GetRequiredService<IMapper<Customer, CustomerInfo>>();

                var sources = new[] {
                    new Customer { Id = 1, FirstName = "John1", LastName = "Doe1", BirthDay = new DateTime(1980, 10, 11), Contact = "Contact1" },
                    new Customer { Id = 2, FirstName = "John2", LastName = "Doe2", BirthDay = new DateTime(1980, 10, 12), Contact = "Contact2" },
                    new Customer { Id = 3, FirstName = "John3", LastName = "Doe3", BirthDay = new DateTime(1980, 10, 13), Contact = "Contact3" },
                    new Customer { Id = 4, FirstName = "John4", LastName = "Doe4", BirthDay = new DateTime(1980, 10, 14), Contact = "Contact4" },
                    new Customer { Id = 5, FirstName = "John5", LastName = "Doe5", BirthDay = new DateTime(1980, 10, 15), Contact = "Contact5" },
                };


                var targets = mapper.Query(sources.AsQueryable()).ToList();

                Assert.All(targets, (p, i) =>
                {
                    Assert.Equal(sources[i].Id, p.Id);
                    Assert.Equal(sources[i].FirstName + " " + sources[i].LastName, p.DisplayName);
                    Assert.Equal(sources[i].Contact, p.Contact);
                });
            }
        }
    }
}
