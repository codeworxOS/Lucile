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

        [Fact]
        public void SourceTargetWithSubMapping_Expects_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                .AddMapping<Country>()
                    .Configure(builder => builder.To(p => new CountryInfo
                    {
                        Id = p.Id,
                        DisplayText = p.Name + " (" + p.Iso2 + ")",
                    }));
            services
                .AddMapping<Address>()
                    .Configure(builder => builder.To(p => new AddressInfo
                    {
                        Id = p.Id,
                        City = p.City,
                        Country = p.Country.Map<Country, CountryInfo>()
                    }));


            using (var sp = services.BuildServiceProvider())
            {
                var mapperInfo = sp.GetRequiredService<IMapper<Address, AddressInfo>>();

                var sources = new[]{
                    new Address { Id = 1, City = "SampleCity", Country = new Country { Id = 2, Iso2 = "AT", Name = "Austria" } },
                };

                var targetInfo = mapperInfo.Query(sources.AsQueryable());

                Assert.All(targetInfo.ToList(), (p, i) =>
                {

                     Assert.Equal(sources[i].Id, p.Id);
                     Assert.Equal(sources[i].City, p.City);

                     Assert.Equal(sources[i].Country.Id, p.Country.Id);
                     Assert.Equal(sources[i].Country.Name + " (" + sources[i].Country.Iso2 + ")", p.Country.DisplayText);
                });
            }
        }
    }
}
