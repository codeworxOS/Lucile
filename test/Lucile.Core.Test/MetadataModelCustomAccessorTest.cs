using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucile.Core.Test.Metadata;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Xunit;

namespace Lucile.Core.Test
{
    public class MetadataModelCustomAccessorTest
    {

        [Fact]
        public void TestCustomAccessor_ExpectsNoError()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Article>();
            builder.Entity<ArticleName>();
            builder.Entity<Contact>();

            var model = builder.ApplyConventions().ToModel(new CustomAccessorFactory());

            Assert.NotNull(model);
        }
    }
}
