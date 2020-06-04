using Lucile.Core.Test;
using Lucile.Data.Metadata;
using Lucile.Data.Metadata.Builder;
using Lucile.Test.Model;
using Newtonsoft.Json;
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
        public void DataContractSerializationPreviousVersionDeserializationTest()
        {
            MetadataModelBuilder builder = null;
            
            using (var ms = typeof(MetadataModelBuilderSerializationTest).Assembly.GetManifestResourceStream("Lucile.Core.Test.Serialization.previous-model.xml"))
            {
                var ser = new DataContractSerializer(typeof(MetadataModelBuilder));
                builder = ser.ReadObject(ms) as MetadataModelBuilder;
            }

            var model = builder.ToModel();

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }

        [Fact]
        public void ProtoBufSerializationPreviousVersionDeserializationTest()
        {
            MetadataModelBuilder builder = null;

            using (var ms = typeof(MetadataModelBuilderSerializationTest).Assembly.GetManifestResourceStream("Lucile.Core.Test.Serialization.previous-model.proto"))
            {
                builder = ProtoBuf.Serializer.Deserialize<MetadataModelBuilder>(ms);
            }

            var model = builder.ToModel();

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }

        [Fact]
        public void JsonSerializationPreviousVersionDeserializationTest()
        {
            MetadataModelBuilder builder = null;

            using (var ms = typeof(MetadataModelBuilderSerializationTest).Assembly.GetManifestResourceStream("Lucile.Core.Test.Serialization.previous-model.json"))
            {
                var json = new JsonSerializerSettings();
                json.Converters.Add(new LucileJsonInheritanceConverter());

                var serializer = JsonSerializer.Create(json);
              
                using (var streamReader = new StreamReader(ms))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    builder = serializer.Deserialize<MetadataModelBuilder>(jsonReader);
                }
            }

            var model = builder.ToModel();

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }


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

        [Fact]
        public void JsonSerializationDeserializationTest()
        {
            var builder = new MetadataModelBuilder();
            builder.Entity<Invoice>();
            builder.Entity<Article>().HasOne(p => p.ArticleSettings).WithPrincipal();
            builder.ApplyConventions();

            MetadataModel model = null;

            using (var ms = new MemoryStream())
            {
                var json = new JsonSerializerSettings();
                json.Converters.Add(new LucileJsonInheritanceConverter());

                var serializer = JsonSerializer.Create(json);
                using (var streamWriter = new StreamWriter(ms, Encoding.UTF8, 512, true))
                {
                    serializer.Serialize(streamWriter, builder);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(ms))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var newBuilder = serializer.Deserialize<MetadataModelBuilder>(jsonReader);
                    model = newBuilder.ToModel();
                }
            }

            TestModelValidations.ValidateInvoiceArticleDefaultModel(model);
        }
    }
}