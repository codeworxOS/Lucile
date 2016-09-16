using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Service
{
    public class KnownTypeProvider
    {
        static KnownTypeProvider()
        {
            serviceContractsLocatorDelegate = DefaultContractsLocator;
        }

        private static Func<IEnumerable<Type>> serviceContractsLocatorDelegate;
        public static void RegisterServiceContractsLocator(Func<IEnumerable<Type>> locator)
        {
            serviceContractsLocatorDelegate = locator;
        }

        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            var types = serviceContractsLocatorDelegate();

            var methods = types
                                .SelectMany(p => p.GetMethods()).ToList();

            var knownTypes = methods.Where(p => p.ReturnType != typeof(void) && p.ReturnType != typeof(Task))
                                .SelectMany(p => GetDataTypes(p.ReturnType))
                                .Union(
                                    methods.SelectMany(p => p.GetParameters())
                                    .SelectMany(p => GetDataTypes(p.ParameterType))
                                ).Distinct()
                                .Where(p => p != typeof(object))
                                .ToList();

            return knownTypes;
        }

        private static IEnumerable<Type> GetDataTypes(Type type)
        {
            if (typeof(Task).IsAssignableFrom(type) && type.IsGenericType) {
                foreach(var t in GetDataTypes(type.GetGenericArguments().First())){
                    yield return t;
                }
                yield break;
            }

            var dictionary = type.GetInterfaces().FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (dictionary != null) {
                foreach(var t in GetDataTypes(dictionary.GetGenericArguments().First())){
                    yield return t;
                }
                foreach(var t in GetDataTypes(dictionary.GetGenericArguments().Last())){
                    yield return t;
                }
                yield break;
            }

            var enumerable = type.GetInterfaces().Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>)).FirstOrDefault();
            if (enumerable == null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                enumerable = type;
            }
            if (type != typeof(string) && enumerable != null)
            {
                foreach(var t in GetDataTypes(enumerable.GetGenericArguments().First()))
                {
                    yield return t;
                }
                yield break;
            }

            if (type.IsPrimitive)
                yield break;

            yield return type;
            yield return type.MakeArrayType();
            //yield return typeof(IEnumerable<>).MakeGenericType(type);
        }

        public static IEnumerable<Type> DefaultContractsLocator()
        {
            var name = new AssemblyName(typeof(string).Assembly.FullName);
            var frameworkToken = name.GetPublicKeyToken();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(p => !p.IsDynamic && !frameworkToken.SequenceEqual(new AssemblyName(p.FullName).GetPublicKeyToken()));

            var serviceContracts = assemblies
                                    .SelectMany(p => p.GetTypes())
                                    .Where(p => p.GetCustomAttributes(typeof(ServiceContractAttribute), false).Any())
                                    .ToList();

            return serviceContracts;
        }
    }
}
