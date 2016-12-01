using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lucile.Data.Metadata.Builder.Convention
{
    public class DefaultStructureConvention : IStructureConvention
    {
        public IDictionary<string, Type> GetNavigations(Type entity)
        {
            var result = new Dictionary<string, Type>();

            foreach (var prop in entity.GetProperties())
            {
                var info = prop.PropertyType.GetTypeInfo();
                if (!info.IsValueType && prop.PropertyType != typeof(string))
                {
                    if (prop.CanRead && prop.CanWrite)
                    {
                        result.Add(prop.Name, GetItemType(prop.PropertyType));
                    }
                    else if (prop.CanRead && prop.PropertyType.IsCollectionType())
                    {
                        result.Add(prop.Name, GetItemType(prop.PropertyType));
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> GetScalarProperties(Type entity)
        {
            return from p in entity.GetProperties()
                   let ti = p.PropertyType.GetTypeInfo()
                   where p.CanRead && p.CanWrite && (ti.IsValueType || p.PropertyType == typeof(string))
                   select p.Name;
        }

        private static Type GetItemType(Type propertyType)
        {
            var enumerable = propertyType.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerable?.GetGenericArguments().First() ?? propertyType;
        }
    }
}