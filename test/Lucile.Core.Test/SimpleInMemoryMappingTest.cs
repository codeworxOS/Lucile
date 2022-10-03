using System;
using System.Linq;
using Lucile.Core.Test.Model.Source;
using Lucile.Core.Test.Model.Target;
using Lucile.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lucile.Core.Test
{
    public class SimpleInMemoryMappingTest
    {
        [Fact]
        public void SourceTargetMapping_Expects_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                .AddMapping<Customer>()
                .Configure(builder => builder.To(p => new CustomerInfo
                {
                    Id = p.Id,
                    DisplayName = p.FirstName + " " + p.LastName,
                }));

            using (var sp = services.BuildServiceProvider())
            {
                var mapper = sp.GetRequiredService<IMapper<Customer, CustomerInfo>>();

                var source = new Customer { Id = 1, FirstName = "John", LastName = "Doe", BirthDay = new DateTime(1980, 10, 10) };
                var target = mapper.Map(source);

                Assert.Equal(source.Id, target.Id);
                Assert.Equal(source.FirstName + " " + source.LastName, target.DisplayName);
            }
        }

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

                var source = new Customer { Id = 1, FirstName = "John", LastName = "Doe", BirthDay = new DateTime(1980, 10, 10), Contact = "Contact" };
                var target = mapper.Map(source);

                Assert.Equal(source.Id, target.Id);
                Assert.Equal(source.FirstName + " " + source.LastName, target.DisplayName);
                Assert.Equal(source.Contact, target.Contact);
            }
        }

        [Fact]
        public void SourceTargetMapping_FromFactory_Expects_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                ////.AddMapping<IOBjectWithTracking>()
                ////    .Configure(builder => builder.To(p => new IOBjectWithTracking
                ////    {
                ////        Id = p.Id,
                ////        DisplayName = p.FirstName + " " + p.LastName,
                ////    }));
                .AddMapping<Customer>()
                    .Configure(builder => builder.To(p => new CustomerInfo
                    {
                        Id = p.Id,
                        DisplayName = p.FirstName + " " + p.LastName,
                    })
                    );

            using (var sp = services.BuildServiceProvider())
            {
                var mapperFactory = sp.GetRequiredService<IMapperFactory>();
                var mapper = mapperFactory.CreateMapper<Customer, CustomerInfo>();

                var source = new Customer { Id = 1, FirstName = "John", LastName = "Doe", BirthDay = new DateTime(1980, 10, 10) };
                var target = mapper.Map(source);

                Assert.Equal(source.Id, target.Id);
                Assert.Equal(source.FirstName + " " + source.LastName, target.DisplayName);
            }
        }


        [Fact]
        public void SourceMutipleTargetMapping_Expects_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                .AddMapping<Customer>()
                    .Configure(builder => builder.To(p => new CustomerInfo
                    {
                        Id = p.Id,
                        DisplayName = p.FirstName + " " + p.LastName,
                    }))
                    .Configure(builder => builder.To(p => new CustomerListItem
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                    }));

            using (var sp = services.BuildServiceProvider())
            {
                var mapperInfo = sp.GetRequiredService<IMapper<Customer, CustomerInfo>>();
                var mapperListItem = sp.GetRequiredService<IMapper<Customer, CustomerListItem>>();

                var source = new Customer { Id = 1, FirstName = "John", LastName = "Doe", BirthDay = new DateTime(1980, 10, 10) };
                var targetInfo = mapperInfo.Map(source);

                Assert.Equal(source.Id, targetInfo.Id);
                Assert.Equal(source.FirstName + " " + source.LastName, targetInfo.DisplayName);

                var targetListItem = mapperListItem.Map(source);

                Assert.Equal(source.Id, targetListItem.Id);
                Assert.Equal(source.FirstName, targetListItem.FirstName);
                Assert.Equal(source.LastName, targetListItem.LastName);
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

                var source = new Address { Id = 1, City = "SampleCity", Country = new Country { Id = 2, Iso2 = "AT", Name = "Austria" } };
                var targetInfo = mapperInfo.Map(source);

                Assert.Equal(source.Id, targetInfo.Id);
                Assert.Equal(source.City, targetInfo.City);

                Assert.Equal(source.Country.Id, targetInfo.Country.Id);
                Assert.Equal(source.Country.Name + " (" + source.Country.Iso2 + ")", targetInfo.Country.DisplayText);


            }
        }

        [Fact]
        public void SourceTargetWithEnumerableSubMapping_Expects_Success()
        {
            var services = new ServiceCollection();

            services
                .AddMapper()
                .AddMapping<Invoice>()
                    .Configure(builder => builder.To(p => new InvoiceInfo
                    {
                        Id = p.Id,
                        Number = p.Number,
                        InvoiceDate = p.InvoiceDate,
                    }));
            services
                .AddMapping<Address>()
                    .Configure(builder => builder.To(p => new AddressInfo
                    {
                        Id = p.Id,
                        City = p.City,
                        Invoices = p.Invoices.Map<Invoice, InvoiceInfo>().OrderBy(p => p.InvoiceDate)
                    }));


            using (var sp = services.BuildServiceProvider())
            {
                var mapperInfo = sp.GetRequiredService<IMapper<Address, AddressInfo>>();

                var source = new Address
                {
                    Id = 1,
                    City = "SampleCity",
                    Invoices = {
                        new Invoice{ Id = 123, Number = "123", InvoiceDate = new DateTime(2020,2,1)},
                        new Invoice{ Id = 124, Number = "124", InvoiceDate = new DateTime(2020,1,1)},
                    }
                };
                var targetInfo = mapperInfo.Map(source);

                Assert.Equal(source.Id, targetInfo.Id);
                Assert.Equal(source.City, targetInfo.City);

                Assert.Equal(2, targetInfo.Invoices.Count());
                Assert.Equal(124, targetInfo.Invoices.First().Id);


            }
        }
    }
}
