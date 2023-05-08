using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration
{
    public class PathValueExpression : ValueExpression
    {
        private static ConcurrentDictionary<Type, ImmutableList<Type>> _knownTypeLookup = new ConcurrentDictionary<Type, ImmutableList<Type>>();

        public PathValueExpression(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public override Expression GetExpression(ParameterExpression parameter)
        {
            Expression result = parameter;
            foreach (var item in Path.Split('.'))
            {
                if (item.StartsWith("<") && item.EndsWith(">"))
                {
                    var typeName = item.Substring(1, item.Length - 2);
                    var knownType = GetKnownTypes(result.Type).FirstOrDefault(p => p.Name == typeName);

                    if (knownType == null)
                    {
                        throw new InvalidOperationException($"Cast target type {typeName} not found in known types of {result.Type}.");
                    }

                    result = Expression.Convert(result, knownType);
                }
                else
                {
                    try
                    {
                        result = Expression.Property(result, item);
                    }
                    catch (ArgumentException)
                    {
                        throw new InvalidPathException(result.Type, item);
                    }
                }
            }

            return result;
        }

        private IEnumerable<Type> GetKnownTypes(Type type)
        {
            var result = _knownTypeLookup.GetOrAdd(type, p => FindKnownTypes(p).ToImmutableList());
            return result;
        }

        private IEnumerable<Type> FindKnownTypes(Type parent)
        {
            var knownTypes = parent.GetTypeInfo().GetCustomAttributes<KnownTypeAttribute>(false);

            foreach (var item in knownTypes.SelectMany(p => GetTypes(p, parent)))
            {
                yield return item;
                foreach (var child in GetKnownTypes(item))
                {
                    yield return child;
                }
            }
        }

        private IEnumerable<Type> GetTypes(KnownTypeAttribute item, Type parent)
        {
            if (item.Type != null)
            {
                yield return item.Type;
            }

            if (item.MethodName != null)
            {
                var methodInfo = parent.GetMethod(item.MethodName, BindingFlags.Public | BindingFlags.Static);
                if (methodInfo == null || methodInfo.ReturnType != typeof(IEnumerable<Type>))
                {
                    throw new InvalidOperationException($"Invlid method declared for known type attribute of type {parent}.");
                }

                foreach (var child in (IEnumerable<Type>)methodInfo.Invoke(null, null))
                {
                    yield return child;
                }
            }
        }
    }
}