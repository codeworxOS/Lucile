using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lucile.Linq.Expressions;

namespace Lucile.Mapper
{
    public abstract class Mapping<TSource, TTarget> : IMappingConfiguration<TSource, TTarget>, IMappingConfiguration
    {
        private Func<TSource, TTarget> _conversionDelegate;
        private Expression<Func<TSource, TTarget>> _conversionExpression;

        Expression IMappingConfiguration.ConversionExpression
        {
            get { return this.Expression; }
        }

        public Expression<Func<TSource, TTarget>> Expression
        {
            get
            {
                EnsureExpression();
                return this._conversionExpression;
            }
        }

        Type IMappingConfiguration.SourceType
        {
            get
            {
                return typeof(TSource);
            }
        }

        Type IMappingConfiguration.TargetType
        {
            get
            {
                return typeof(TTarget);
            }
        }

        bool IMappingConfiguration.CanConvert(object source)
        {
            return source is TSource;
        }

        public bool CanConvertBack()
        {
            EnsureExpression();
            return false;
        }

        bool IMappingConfiguration.CanConvertType(Type sourceType)
        {
            return typeof(TSource).IsAssignableFrom(sourceType);
        }

        public TTarget Convert(TSource source)
        {
            EnsureExpression();
            return this._conversionDelegate(source);
        }

        object IMappingConfiguration.Convert(object source)
        {
            return this.Convert((TSource)source);
        }

        public TSource ConvertBack(TTarget target)
        {
            EnsureExpression();
            ////TODO implement;
            return default(TSource);
        }

        protected abstract Expression<Func<TSource, TTarget>> GetConversionExpression();

        protected MappingAggregator<TSource, TTarget> MapBaseMapping()
        {
            return new MappingAggregator<TSource, TTarget>().BaseMapping();
        }

        protected MappingAggregator<TSource, TTarget> MapBaseMapping<TBaseTarget>()
        {
            return new MappingAggregator<TSource, TTarget>().BaseMapping<TBaseTarget>();
        }

        protected MappingAggregator<TSource, TTarget> MapBaseMapping(Type baseTargetType)
        {
            return new MappingAggregator<TSource, TTarget>().BaseMapping(baseTargetType);
        }

        protected MappingAggregator<TSource, TTarget> MapBaseType()
        {
            return new MappingAggregator<TSource, TTarget>().BaseType();
        }

        protected MappingAggregator<TSource, TTarget> MapBaseType<TBaseType>()
        {
            return new MappingAggregator<TSource, TTarget>().BaseType<TBaseType>();
        }

        protected MappingAggregator<TSource, TTarget> MapBaseType(Type baseType)
        {
            return new MappingAggregator<TSource, TTarget>().BaseType(baseType);
        }

        private void EnsureExpression()
        {
            if (_conversionExpression == null)
            {
                _conversionExpression = GetConversionExpression();
                var visitor = new FindMapExpressionVisitor();
                visitor.Visit(_conversionExpression);
                foreach (var item in visitor.MapCalls)
                {
                    var param = System.Linq.Expressions.Expression.Parameter(typeof(MappingContainer));
                    var body = System.Linq.Expressions.Expression.Call(
                        param,
                        MappingContainer.MethodInfoCache.GetMappingOrDefaultMethod.MakeGenericMethod(item.Method.GetGenericArguments()),
                        System.Linq.Expressions.Expression.Default(item.Method.GetGenericArguments().First()));

                    var getMappingDelegate = System.Linq.Expressions.Expression.Lambda<Func<MappingContainer, IMappingConfiguration>>(body, param).Compile();
                    var mapping = getMappingDelegate(MappingContainer.Current);
                    if (mapping == null)
                    {
                        throw new InvalidMappingException(item.Method.GetGenericArguments().First(), item.Method.GetGenericArguments().Last());
                    }

                    var lambda = (LambdaExpression)mapping.ConversionExpression;

                    if (item.Method.ReturnType == typeof(IQueryable<>).MakeGenericType(item.Method.GetGenericArguments().Last()))
                    {
                        this._conversionExpression = _conversionExpression.Replace(
                            item,
                            System.Linq.Expressions.Expression.Call(
                                MappingContainer.MethodInfoCache.QueryableSelectMethod.MakeGenericMethod(item.Method.GetGenericArguments()),
                                item.Arguments.First(),
                                mapping.ConversionExpression));
                    }
                    else if (item.Method.ReturnType == typeof(IEnumerable<>).MakeGenericType(item.Method.GetGenericArguments().Last()))
                    {
                        this._conversionExpression = _conversionExpression.Replace(
                            item,
                            System.Linq.Expressions.Expression.Call(
                                MappingContainer.MethodInfoCache.EnumerableSelectMethod.MakeGenericMethod(item.Method.GetGenericArguments()),
                                item.Arguments.First(),
                                mapping.ConversionExpression));
                    }
                    else
                    {
                        var newMapping = lambda.Body.Replace(((LambdaExpression)mapping.ConversionExpression).Parameters.First(), item.Arguments.First());

                        this._conversionExpression = _conversionExpression.Replace(item, newMapping);
                    }
                }

                _conversionDelegate = this._conversionExpression.Compile();
            }
        }
    }
}