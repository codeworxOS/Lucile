using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyNavigationBuilder<TSource, TTarget> : ManyNavigationBuilder
        where TSource : class
        where TTarget : class
    {
        public ManyNavigationBuilder(EntityMetadataBuilder entityBuilder, string propertyName = null)
            : base(entityBuilder, typeof(TTarget), propertyName)
        {
        }

        public ManyToManyNavigationBuilder WithMany(Expression<Func<TTarget, IEnumerable<TSource>>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return WithMany(propertyName);
        }

        public ManyToOneNavigationBuilder<TSource, TTarget> WithOne(Expression<Func<TTarget, TSource>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return new ManyToOneNavigationBuilder<TSource, TTarget>(this, propertyName);
        }
    }
}