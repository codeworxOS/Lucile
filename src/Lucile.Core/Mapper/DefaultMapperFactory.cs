using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Mapper
{
    public class DefaultMapperFactory : IMapperFactory
    {
        private ImmutableList<IMappingConfiguration> _mappings;

        private ConcurrentDictionary<object, ConcurrentDictionary<object, IMapper>> _mapperCache = new ConcurrentDictionary<object, ConcurrentDictionary<object, IMapper>>();

        public DefaultMapperFactory(IEnumerable<IMappingConfiguration> mappings)
        {
            _mappings = mappings.ToImmutableList();
        }

        public IEnumerable<IMappingConfiguration> Mappings
        {
            get
            {
                return _mappings;
            }
        }

        public IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>()
        {
            var sourceMappers = _mapperCache.GetOrAdd(TypeKey<TSource>.Key, p => new ConcurrentDictionary<object, IMapper>());
            var targetMapper = sourceMappers.GetOrAdd(TypeKey<TTarget>.Key, p =>
            {
                var configuration = _mappings.OfType<IMappingConfiguration<TSource, TTarget>>().FirstOrDefault();

                if (configuration == null)
                {
                    throw new InvalidMappingException(typeof(TSource), typeof(TTarget));
                }

                return new DefaultMapper<TSource, TTarget>(configuration);
            });

            return (IMapper<TSource, TTarget>)targetMapper;
        }

        public IMapper CreateMapper(Type sourceType, Type targetType)
        {
            var sourceKey = TypeKey.ForType(sourceType);
            var targetKey = TypeKey.ForType(targetType);
            var sourceMappers = _mapperCache.GetOrAdd(sourceKey, p => new ConcurrentDictionary<object, IMapper>());
            var targetMapper = sourceMappers.GetOrAdd(targetKey, p =>
            {
                return (IMapper)MethodInfoCache.CreateMapperMethod.MakeGenericMethod(sourceType, targetType).Invoke(this, null);
            });
            return targetMapper;
        }

        public IEnumerable<IMapper> GetMappers(Type sourceType)
        {
            foreach (var item in _mappings.Where(p => p.CanConvertType(sourceType)))
            {
                yield return CreateMapper(sourceType, item.TargetType);
            }
        }

        ////private IEnumerable<IMappingConfiguration> GetMappings(object source)
        ////{
        ////    return this._mappings.Where(p => p.CanConvert(source)).ToList();
        ////}

        ////private IEnumerable<IMappingConfiguration> GetMappings(Type sourceType)
        ////{
        ////    return this._mappings.Where(p => p.CanConvertType(sourceType)).ToList();
        ////}

        ////private TTarget Map<TSource, TTarget>(TSource source)
        ////{
        ////    TTarget value;
        ////    var canMap = TryMap<TSource, TTarget>(source, out value);
        ////    if (!canMap)
        ////    {
        ////        throw new InvalidMappingException(typeof(TSource), typeof(TTarget));
        ////    }

        ////    return value;
        ////}

        ////private IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> source)
        ////{
        ////    return source.Select(p => p.Map<TSource, TTarget>());
        ////}

        ////private IQueryable<TTarget> Map<TSource, TTarget>(IQueryable<TSource> query)
        ////{
        ////    var mapping = this._mappings.OfType<IMappingConfiguration<TSource, TTarget>>().Single();

        ////    return query.Select<TSource, TTarget>(mapping.Expression);
        ////}

        ////private IEnumerable<TTarget> MapAggregate<TSource, TTarget>(IEnumerable<TSource> sourceItems)
        ////{
        ////    return MapAggregate<TSource, TTarget>(sourceItems.ToArray());
        ////}

        ////private IEnumerable<TTarget> MapAggregate<TSource, TTarget>(params TSource[] sourceItems)
        ////{
        ////    Dictionary<TSource, IMappingConfiguration> mappings = new Dictionary<TSource, IMappingConfiguration>();

        ////    foreach (var item in sourceItems)
        ////    {
        ////        IMappingConfiguration mapping;
        ////        if (this.TryGetMapping<TSource, TTarget>(item, out mapping))
        ////        {
        ////            mappings.Add(item, mapping);
        ////        }
        ////        else
        ////        {
        ////            throw new InvalidOperationException(string.Format("Multiple or no mappingConfiguration found for source type [{0}] and target type [{1}].", sourceItems.GetType(), typeof(TTarget)));
        ////        }
        ////    }

        ////    foreach (var targetGroup in mappings.GroupBy(p => p.Value.TargetType).ToList())
        ////    {
        ////        if (targetGroup.Count() == 1)
        ////        {
        ////            var item = targetGroup.First();
        ////            yield return (TTarget)item.Value.Convert(item.Key);
        ////        }
        ////        else
        ////        {
        ////            List<object> sources = new List<object>();
        ////            List<ParameterExpression> parameters = new List<ParameterExpression>();
        ////            List<MemberBinding> members = new List<MemberBinding>();
        ////            NewExpression newExpression = null;

        ////            foreach (var item in targetGroup)
        ////            {
        ////                sources.Add(item.Key);
        ////                var lambda = item.Value.ConversionExpression as LambdaExpression;
        ////                if (lambda != null)
        ////                {
        ////                    parameters.AddRange(lambda.Parameters);
        ////                    var memberInit = lambda.Body as MemberInitExpression;
        ////                    if (memberInit != null)
        ////                    {
        ////                        members.AddRange(memberInit.Bindings);
        ////                    }

        ////                    if (newExpression == null && memberInit != null)
        ////                    {
        ////                        newExpression = memberInit.NewExpression;
        ////                    }
        ////                    else if (newExpression == null)
        ////                    {
        ////                        newExpression = lambda.Body as NewExpression;
        ////                    }
        ////                }
        ////            }

        ////            var result = (TTarget)Expression.Lambda(Expression.MemberInit(newExpression, members), parameters).Compile().DynamicInvoke(sources.ToArray());

        ////            yield return result;
        ////        }
        ////    }
        ////}

        ////private bool TryGetMapping<TSource, TTarget>(TSource source, out IMappingConfiguration mapping)
        ////{
        ////    mapping = null;
        ////    var mappings = this._mappings.OfType<IMappingConfiguration<TSource, TTarget>>().ToList();

        ////    if (mappings.Any())
        ////    {
        ////        if (mappings.Count > 1)
        ////        {
        ////            return false;
        ////        }

        ////        mapping = mappings[0];
        ////        return true;
        ////    }
        ////    else
        ////    {
        ////        return TryGetMapping(source.GetType(), typeof(TTarget), out mapping);
        ////    }
        ////}

        ////private bool TryGetMapping(Type source, Type target, out IMappingConfiguration mapping)
        ////{
        ////    mapping = null;
        ////    var untypedMappings = this._mappings.Where(p => p.CanConvertType(source))
        ////            .Where(p => target.IsAssignableFrom(p.TargetType))
        ////            .ToList();

        ////    if (untypedMappings.Any())
        ////    {
        ////        return TryGetRankedMapping(source, untypedMappings, out mapping);
        ////    }

        ////    return false;
        ////}

        ////private bool TryMap<TSource, TTarget>(TSource source, out TTarget target)
        ////{
        ////    target = default(TTarget);
        ////    IMappingConfiguration mapping;
        ////    if (TryGetMapping<TSource, TTarget>(source, out mapping))
        ////    {
        ////        target = (TTarget)mapping.Convert(source);
        ////        return true;
        ////    }

        ////    return false;
        ////}

        ////private bool TryGetRankedMapping(Type source, IEnumerable<IMappingConfiguration> mappings, out IMappingConfiguration mapping)
        ////{
        ////    mapping = null;
        ////    if (mappings.Count() > 1)
        ////    {
        ////        var tempMappings = mappings
        ////            .Join(
        ////                source.GetBaseTypeStructure(),
        ////                p => p.SourceType,
        ////                p => p.Value,
        ////                (p, q) => new { Index = q.Key, mapping = p })
        ////            .OrderBy(p => p.Index).ToList();

        ////        if (tempMappings.First().Index != tempMappings.ElementAt(1).Index)
        ////        {
        ////            mapping = tempMappings.Select(p => p.mapping).FirstOrDefault();
        ////            return true;
        ////        }
        ////        else
        ////        {
        ////            return false;
        ////        }
        ////    }

        ////    mapping = mappings.FirstOrDefault();
        ////    return mapping != null;
        ////}

        internal class MethodInfoCache
        {
            static MethodInfoCache()
            {
                Expression<Func<IMapperFactory, IMapper<object, object>>> exp = p => p.CreateMapper<object, object>();

                CreateMapperMethod = ((MethodCallExpression)exp.Body).Method.GetGenericMethodDefinition();
                Expression<Func<IQueryable<string>, IQueryable<string>>> queryableSelect = p => p.Select(x => x);
                Expression<Func<IEnumerable<string>, IEnumerable<string>>> enumerableSelect = p => p.Select(x => x);
                QueryableSelectMethod = ((MethodCallExpression)queryableSelect.Body).Method.GetGenericMethodDefinition();
                EnumerableSelectMethod = ((MethodCallExpression)enumerableSelect.Body).Method.GetGenericMethodDefinition();
            }

            public static MethodInfo EnumerableSelectMethod { get; }

            public static MethodInfo CreateMapperMethod { get; }

            public static MethodInfo QueryableSelectMethod { get; }
        }
    }
}
