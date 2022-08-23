using System;
using System.Linq.Expressions;

namespace Lucile.Mapper
{
    public interface IMappingConfiguration
    {
        Type TargetType { get; }

        Type SourceType { get; }

        LambdaExpression ConversionExpression { get; }

        bool CanConvert(object source);

        bool CanConvertType(Type sourceType);

        object Convert(object source);
    }

    public interface IMappingConfiguration<TSource, TTarget> : IMappingConfiguration
    {
        Expression<Func<TSource, TTarget>> Expression { get; }

        TTarget Convert(TSource source);

        bool CanConvertBack();

        TSource ConvertBack(TTarget target);
    }
}
