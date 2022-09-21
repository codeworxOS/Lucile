using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Mapper
{
    public class DefaultMappingConfiguration<TSource, TTarget> : IMappingConfiguration<TSource, TTarget>, IMappingConfiguration
    {
        private Func<TSource, TTarget> _conversionDelegate;

        private Expression<Func<TSource, TTarget>> _conversionExpression;

        public DefaultMappingConfiguration(Expression<Func<TSource, TTarget>> conversionExpression)
        {
            _conversionExpression = conversionExpression;
            _conversionDelegate = _conversionExpression.Compile();
        }

        LambdaExpression IMappingConfiguration.ConversionExpression
        {
            get { return this.Expression; }
        }

        public Expression<Func<TSource, TTarget>> Expression
        {
            get
            {
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
            return false;
        }

        bool IMappingConfiguration.CanConvertType(Type sourceType)
        {
            return typeof(TSource).IsAssignableFrom(sourceType);
        }

        public TTarget Convert(TSource source)
        {
            return this._conversionDelegate(source);
        }

        object IMappingConfiguration.Convert(object source)
        {
            return this.Convert((TSource)source);
        }

        public TSource ConvertBack(TTarget target)
        {
            ////TODO implement;
            return default(TSource);
        }
    }
}