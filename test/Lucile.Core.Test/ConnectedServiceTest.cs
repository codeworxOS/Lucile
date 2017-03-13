using System;
using System.Linq;
using System.Reflection;
using Lucile.Core.Test.Module1;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class ConnectedServiceTest
    {
        [Fact]
        public void AssemblyBootstrap()
        {
            var collection = new ServiceCollection();
            collection.FromConfiguration(typeof(ITestService).GetTypeInfo().Assembly);

            var provider = collection.BuildServiceProvider();
            var service = provider.GetService<ITestService>();

            Assert.IsType(typeof(TestService), service);
        }
    }
}