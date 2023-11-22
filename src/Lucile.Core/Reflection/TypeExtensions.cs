using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class TypeExtensions
    {
        public static IDictionary<int, Type> GetBaseTypeStructure(this Type type)
        {
            var dict = new Dictionary<int, Type>();
            GetBaseClassStructure(type, dict);
            GetBaseInterfaceStructure(type, dict, dict.Keys.Count);
            return dict;
        }

        public static string GetFriendlyName(this Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                var friendlyName = type.Name;

                int indexBacktick = friendlyName.IndexOf('`');
                if (indexBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(indexBacktick);
                }

                return $"{friendlyName}<{string.Join(",", type.GetGenericArguments().Select(p => p.GetFriendlyName()))}>";
            }

            return type.Name;
        }

        public static bool IsCollectionType(this Type type)
        {
            Type elementType;
            return IsCollectionType(type, out elementType);
        }

        public static bool IsCollectionType(this Type type, out Type itemType)
        {
            var enumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? type : null;

            enumerable = enumerable ?? type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(p => p.GetTypeInfo().IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerable != null)
            {
                itemType = enumerable.GetGenericArguments().First();
                return true;
            }

            itemType = type;
            return false;
        }

        private static void GetBaseClassStructure(Type type, Dictionary<int, Type> dict, int priority = 0)
        {
            if (type == typeof(object))
            {
                return;
            }

            dict.Add(priority, type);

            GetBaseClassStructure(type.GetTypeInfo().BaseType, dict, priority + 1);
        }

        private static void GetBaseInterfaceStructure(Type type, Dictionary<int, Type> dict, int priority)
        {
            var interfaces = type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                dict.Add(priority + i + 1, interfaces[i]);
            }
        }
    }
}