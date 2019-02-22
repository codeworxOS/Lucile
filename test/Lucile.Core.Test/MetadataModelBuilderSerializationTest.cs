using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace Tests
{
    public class MetadataModelBuilderSerializationTest
    {
        [Fact]
        public void DataContractSerializationDeserializationTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>();
            builder.Entity<Article>().HasOne(p => p.ArticleSettings).WithPrincipal();
            builder.ApplyConventions();

            MetadataModel model = null;

            using (var ms = new MemoryStream())
            {
                var ser = new DataContractSerializer(typeof(MetadataModelBuilder));
                ser.WriteObject(ms, builder);
                ms.Seek(0, SeekOrigin.Begin);
                var newBuilder = (MetadataModelBuilder)ser.ReadObject(ms);
                model = newBuilder.ToModel();
            }

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }

        [Fact]
        public void ProtoBufSerializationDeserializationTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>();
            builder.Entity<Article>().HasOne(p => p.ArticleSettings).WithPrincipal();
            builder.ApplyConventions();

            MetadataModel model = null;

            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<MetadataModelBuilder>(ms, builder);

                ms.Seek(0, SeekOrigin.Begin);

                var newBuilder = ProtoBuf.Serializer.Deserialize<MetadataModelBuilder>(ms);
                model = newBuilder.ToModel();
            }

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }
    }
}