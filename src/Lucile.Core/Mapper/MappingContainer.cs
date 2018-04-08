using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Mapper
{
    public class MappingContainer
    {
        private static MappingContainer _current;

#pragma warning disable 0649

        ////        [ImportMany(AllowRecomposition = true)]
        private List<IMappingConfiguration> _mappings;

#pragma warning restore 0649

        private MappingContainer()
        {
            ////CompositionContext.Current.ComposeParts(this);
        }

        public static MappingContainer Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new MappingContainer();
                }

                return _current;
            }
        }

        public IEnumerable<IMappingConfiguration> Mappings
        {
            get
            {
                return _mappings;
            }
        }

        public IMappingConfiguration GetMappingOrDefault<TSource, TTarget>(TSource source)
        {
            IMappingConfiguration mapping = null;
            TryGetMapping<TSource, TTarget>(source, out mapping);
            return mapping;
        }

        public IEnumerable<IMappingConfiguration> GetMappings(object source)
        {
            return this._mappings.Where(p => p.CanConvert(source)).ToList();
        }

        public IEnumerable<IMappingConfiguration> GetMappings(Type sourceType)
        {
            return this._mappings.Where(p => p.CanConvertType(sourceType)).ToList();
        }

        public TTarget Map<TSource, TTarget>(TSource source)
        {
            TTarget value;
            var canMap = TryMap<TSource, TTarget>(source, out value);
            if (!canMap)
            {
                throw new InvalidMappingException(typeof(TSource), typeof(TTarget));
            }

            return value;
        }

        public IEnumerable<TTarget> Map<TSource, TTarget>(IEnumerable<TSource> source)
        {
            return source.Select(p => p.Map<TSource, TTarget>());
        }

        public IQueryable<TTarget> Map<TSource, TTarget>(IQueryable<TSource> query)
        {
            var mapping = this._mappings.OfType<IMappingConfiguration<TSource, TTarget>>().Single();

            return query.Select<TSource, TTarget>(mapping.Expression);
        }

        public IEnumerable<TTarget> MapAggregate<TSource, TTarget>(IEnumerable<TSource> sourceItems)
        {
            return MapAggregate<TSource, TTarget>(sourceItems.ToArray());
        }

        public IEnumerable<TTarget> MapAggregate<TSource, TTarget>(params TSource[] sourceItems)
        {
            Dictionary<TSource, IMappingConfiguration> mappings = new Dictionary<TSource, IMappingConfiguration>();

            foreach (var item in sourceItems)
            {
                IMappingConfiguration mapping;
                if (this.TryGetMapping<TSource, TTarget>(item, out mapping))
                {
                    mappings.Add(item, mapping);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Multiple or no mappingConfiguration found for source type [{0}] and target type [{1}].", sourceItems.GetType(), typeof(TTarget)));
                }
            }

            foreach (var targetGroup in mappings.GroupBy(p => p.Value.TargetType).ToList())
            {
                if (targetGroup.Count() == 1)
                {
                    var item = targetGroup.First();
                    yield return (TTarget)item.Value.Convert(item.Key);
                }
                else
                {
                    List<object> sources = new List<object>();
                    List<ParameterExpression> parameters = new List<ParameterExpression>();
                    List<MemberBinding> members = new List<MemberBinding>();
                    NewExpression newExpression = null;

                    foreach (var item in targetGroup)
                    {
                        sources.Add(item.Key);
                        var lambda = item.Value.ConversionExpression as LambdaExpression;
                        if (lambda != null)
                        {
                            parameters.AddRange(lambda.Parameters);
                            var memberInit = lambda.Body as MemberInitExpression;
                            if (memberInit != null)
                            {
                                members.AddRange(memberInit.Bindings);
                            }

                            if (newExpression == null && memberInit != null)
                            {
                                newExpression = memberInit.NewExpression;
                            }
                            else if (newExpression == null)
                            {
                                newExpression = lambda.Body as NewExpression;
                            }
                        }
                    }

                    var result = (TTarget)Expression.Lambda(Expression.MemberInit(newExpression, members), parameters).Compile().DynamicInvoke(sources.ToArray());

                    yield return result;
                }
            }
        }

        public bool TryGetMapping<TSource, TTarget>(TSource source, out IMappingConfiguration mapping)
        {
            mapping = null;
            var mappings = this._mappings.OfType<IMappingConfiguration<TSource, TTarget>>().ToList();

            if (mappings.Any())
            {
                if (mappings.Count > 1)
                {
                    return false;
                }

                mapping = mappings[0];
                return true;
            }
            else
            {
                return TryGetMapping(source.GetType(), typeof(TTarget), out mapping);
            }
        }

        public bool TryGetMapping(Type source, Type target, out IMappingConfiguration mapping)
        {
            mapping = null;
            var untypedMappings = this._mappings.Where(p => p.CanConvertType(source))
                    .Where(p => target.IsAssignableFrom(p.TargetType))
                    .ToList();

            if (untypedMappings.Any())
            {
                return TryGetRankedMapping(source, untypedMappings, out mapping);
            }

            return false;
        }

        public bool TryMap<TSource, TTarget>(TSource source, out TTarget target)
        {
            target = default(TTarget);
            IMappingConfiguration mapping;
            if (TryGetMapping<TSource, TTarget>(source, out mapping))
            {
                target = (TTarget)mapping.Convert(source);
                return true;
            }

            return false;
        }

        internal bool TryGetRankedMapping(Type source, IEnumerable<IMappingConfiguration> mappings, out IMappingConfiguration mapping)
        {
            mapping = null;
            if (mappings.Count() > 1)
            {
                var tempMappings = mappings
                    .Join(
                        source.GetBaseTypeStructure(),
                        p => p.SourceType,
                        p => p.Value,
                        (p, q) => new { Index = q.Key, mapping = p })
                    .OrderBy(p => p.Index).ToList();

                if (tempMappings.First().Index != tempMappings.ElementAt(1).Index)
                {
                    mapping = tempMappings.Select(p => p.mapping).FirstOrDefault();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            mapping = mappings.FirstOrDefault();
            return mapping != null;
        }

        internal class MethodInfoCache
        {
            static MethodInfoCache()
            {
                GetMappingOrDefaultMethod = typeof(MappingContainer).GetMethod("GetMappingOrDefault");
                Expression<Func<IQueryable<string>, IQueryable<string>>> queryableSelect = p => p.Select(x => x);
                Expression<Func<IEnumerable<string>, IEnumerable<string>>> enumerableSelect = p => p.Select(x => x);
                QueryableSelectMethod = ((MethodCallExpression)queryableSelect.Body).Method.GetGenericMethodDefinition();
                EnumerableSelectMethod = ((MethodCallExpression)enumerableSelect.Body).Method.GetGenericMethodDefinition();
            }

            public static MethodInfo EnumerableSelectMethod { get; }

            public static MethodInfo GetMappingOrDefaultMethod { get; }

            public static MethodInfo QueryableSelectMethod { get; }
        }
    }
}