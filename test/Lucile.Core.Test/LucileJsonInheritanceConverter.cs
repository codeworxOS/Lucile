using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lucile.Core.Test
{
        public class LucileJsonInheritanceConverter : NJsonSchema.Converters.JsonInheritanceConverter
        {
            public LucileJsonInheritanceConverter()
                : base("type")
            {
            }



            public override bool CanConvert(Type objectType)
            {
                if (objectType == typeof(Lucile.Linq.Configuration.Builder.FilterItemBuilder))
                {
                    System.Diagnostics.Debug.WriteLine("FilterItem");
                }



                var jsonConverterAttribute = objectType.GetTypeInfo().GetCustomAttribute<Lucile.Json.JsonConverterAttribute>(true);
                if (jsonConverterAttribute != null)
                {
                    return typeof(Lucile.Json.JsonInheritanceConverter).IsAssignableFrom(jsonConverterAttribute.ConverterType);
                }



                return false;
            }
        }
}
