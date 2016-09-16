using System.Collections.Generic;
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