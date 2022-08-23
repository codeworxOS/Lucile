using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Lucile.Mapper
{
    public class DefaultMappingBuilder<TSource> : IMappingBuilder<TSource>
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultMappingBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMappingBuilder<TSource, TTarget> Auto<TTarget>()
        {
            return new DefaultMappingBuilder<TSource, TTarget>(_serviceProvider, Expression.Lambda<Func<TSource, TTarget>>(Expression.New(typeof(TTarget)), Expression.Parameter(typeof(TSource), "p")));
        }

        public IMappingBuilder<TSource, TTarget> Base<TTarget>()
        {
            var mapping = _serviceProvider.GetService<IMappingConfiguration<TSource, TTarget>>();

            return new DefaultMappingBuilder<TSource, TTarget>(_serviceProvider, mapping.Expression);
        }

        public IMappingBuilder<TSource, TTarget> To<TTarget>(System.Linq.Expressions.Expression<Func<TSource, TTarget>> mapping)
        {
            return new DefaultMappingBuilder<TSource, TTarget>(_serviceProvider, mapping);
        }
    }
}
