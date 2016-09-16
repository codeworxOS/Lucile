using System;
using System.Linq.Expressions;

namespace Lucile.Mapper
{
    ////[InheritedExport(typeof(IMappingConfiguration))]
    public interface IMappingConfiguration
    {
        Type TargetType { get; }

        Type SourceType { get; }

        Expression ConversionExpression { get; }

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
