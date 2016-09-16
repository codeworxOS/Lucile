using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Linq.Expressions;

namespace Lucile.Mapper
{
    public class MappingAggregator<TSource, TTarget>
    {
        private List<Type> _baseTypeMappings;

        private List<IMappingConfiguration> _baseMappings;

        public MappingAggregator()
        {
            this._baseTypeMappings = new List<Type>();
            this._baseMappings = new List<IMappingConfiguration>();
        }

        public MappingAggregator<TSource, TTarget> BaseMapping()
        {
            IMappingConfiguration configuration;

            var sourceType = typeof(TSource);

            var sources = MappingContainer.Current.GetMappings(sourceType);

            foreach (var item in typeof(TTarget).GetBaseTypeStructure().OrderBy(p => p.Key))
            {
                if (MappingContainer.Current.TryGetRankedMapping(sourceType, sources.Where(p => p.TargetType == item.Value), out configuration))
                {
                    this._baseMappings.Add(configuration);
                    break;
                }
            }

            return this;
        }

        public MappingAggregator<TSource, TTarget> BaseMapping<TMappingTarget>()
        {
            var mapping = MappingContainer.Current.Mappings.OfType<IMappingConfiguration<TSource, TMappingTarget>>().FirstOrDefault();

            if (mapping != null)
            {
                this._baseMappings.Add(mapping);
            }
            else
            {
                return this.BaseMapping(typeof(TMappingTarget));
            }

            return this;
        }

        public MappingAggregator<TSource, TTarget> BaseMapping(Type mappingTarget)
        {
            IMappingConfiguration configuration;

            var sourceType = typeof(TSource);
            var sources = MappingContainer.Current.GetMappings(sourceType).Where(p => p.TargetType == mappingTarget);

            if (MappingContainer.Current.TryGetRankedMapping(sourceType, sources, out configuration))
            {
                this._baseMappings.Add(configuration);
                return this;
            }

            throw new InvalidCastException(string.Format("No base mapping found for source Type {0} and targetType {1}.", typeof(TSource), mappingTarget));
        }

        public MappingAggregator<TSource, TTarget> BaseType()
        {
            Type baseType = null;
            var sourceStructure = typeof(TSource).GetBaseTypeStructure();
            var targetStructure = typeof(TTarget).GetBaseTypeStructure();

            baseType = (from s in sourceStructure
                        join t in targetStructure on s.Value equals t.Value
                        orderby s.Key
                        select s.Value).FirstOrDefault();

            return BaseType(baseType);
        }

        public MappingAggregator<TSource, TTarget> BaseType<TBaseType>()
        {
            return this.BaseType(typeof(TBaseType));
        }

        public MappingAggregator<TSource, TTarget> BaseType(Type baseType)
        {
            if (baseType.IsAssignableFrom(typeof(TSource)) && baseType.IsAssignableFrom(typeof(TTarget)))
            {
                this._baseTypeMappings.Add(baseType);

                return this;
            }

            throw new InvalidCastException(string.Format("TBaseType must be a shared base Type of {0} and {1}", typeof(TSource), typeof(TTarget)));
        }

        public Expression<Func<TSource, TTarget>> Create(Expression<Func<TSource, TTarget>> translation)
        {
            var parameter = translation.Parameters.First();
            Dictionary<string, Expression> members = new Dictionary<string, Expression>();

            foreach (var item in _baseTypeMappings)
            {
                item.GetProperties().Where(p => p.CanRead && p.GetSetMethod() != null)
                    .ToList()
                    .ForEach(p => members[p.Name] = Expression.Property(parameter, p));
            }

            foreach (var mapping in _baseMappings)
            {
                var lambda = mapping.ConversionExpression as LambdaExpression;
                if (lambda != null)
                {
                    var init = lambda.Body as MemberInitExpression;
                    var visitor = new ReplaceExpressionVisitor(lambda.Parameters.First(), translation.Parameters.First());
                    if (init != null)
                    {
                        foreach (var item in init.Bindings.OfType<MemberAssignment>())
                        {
                            members[item.Member.Name] = visitor.Visit(item.Expression);
                        }
                    }
                }
            }

            var memberInit = translation.Body as MemberInitExpression;
            var newExpression = memberInit != null ? memberInit.NewExpression : translation.Body as NewExpression;

            if (memberInit == null && newExpression == null)
            {
                throw new ArgumentOutOfRangeException("translation", "Only New and MemberInit Expressions are allowed");
            }

            if (memberInit != null)
            {
                memberInit.Bindings.ToList()
                    .OfType<MemberAssignment>()
                    .ToList()
                    .ForEach(p => members[p.Member.Name] = p.Expression);
            }

            var targetType = typeof(TTarget);

            var newInit = Expression.MemberInit(newExpression, members.Select(p => Expression.Bind(targetType.GetProperty(p.Key), p.Value)));

            return Expression.Lambda<Func<TSource, TTarget>>(newInit, translation.Parameters);
        }

        public Expression<Func<TSource, TTarget>> Create()
        {
            var param = Expression.Parameter(typeof(TSource));
            var body = Expression.New(typeof(TTarget));

            var lambda = Expression.Lambda<Func<TSource, TTarget>>(body, param);

            return Create(lambda);
        }

        public IMappingConfiguration<TSource, TTarget> CreateMapping(Expression<Func<TSource, TTarget>> translation)
        {
            return new AggregatedMappingConfiguration<TSource, TTarget>(Create(translation));
        }

        public IMappingConfiguration<TSource, TTarget> CreateMapping()
        {
            return new AggregatedMappingConfiguration<TSource, TTarget>(Create());
        }

        ////[PartNotDiscoverable]
        private class AggregatedMappingConfiguration<TS, TT> : Mapping<TS, TT>
        {
            private Expression<Func<TS, TT>> _lambda;

            public AggregatedMappingConfiguration(Expression<Func<TS, TT>> lambda)
            {
                this._lambda = lambda;
            }

            protected override Expression<Func<TS, TT>> GetConversionExpression()
            {
                return this._lambda;
            }
        }
    }
}
