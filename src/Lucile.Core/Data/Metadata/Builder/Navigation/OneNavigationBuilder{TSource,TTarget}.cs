using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneNavigationBuilder<TSource, TTarget> : OneNavigationBuilder
        where TTarget : class
        where TSource : class
    {
        public OneNavigationBuilder(EntityMetadataBuilder entityBuilder, string propertyName = null)
            : base(entityBuilder, typeof(TTarget), propertyName)
        {
        }

        public new OneNavigationBuilder<TSource, TTarget> Required(bool value = true)
        {
            base.Required(value);
            return this;
        }

        public OneToOneNavigationBuilder WithDependant(Expression<Func<TTarget, TSource>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return WithDependant(propertyName);
        }

        public OneToManyNavigationBuilder<TSource, TTarget> WithMany(Expression<Func<TTarget, IEnumerable<TSource>>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return new OneToManyNavigationBuilder<TSource, TTarget>(this, propertyName);
        }

        public OneToOneNavigationBuilder WithPrincipal(Expression<Func<TTarget, TSource>> propertySelector)
        {
            var propertyName = propertySelector.GetPropertyName();
            return WithPrincipal(propertyName);
        }
    }
}