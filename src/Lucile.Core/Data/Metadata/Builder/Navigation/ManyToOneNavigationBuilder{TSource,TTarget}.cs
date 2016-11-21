namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class ManyToOneNavigationBuilder<TSource, TTarget> : ManyToOneNavigationBuilder
        where TSource : class
        where TTarget : class
    {
        public ManyToOneNavigationBuilder(ManyNavigationBuilder manyNavigationBuilder, string propertyName = null)
            : base(manyNavigationBuilder, propertyName)
        {
        }

        public new ManyToOneNavigationBuilder<TSource, TTarget> HasForeignKey(params string[] properties)
        {
            return (ManyToOneNavigationBuilder<TSource, TTarget>)base.HasForeignKey(properties);
        }

        public new ManyToOneNavigationBuilder<TSource, TTarget> Optional(bool value = true)
        {
            return (ManyToOneNavigationBuilder<TSource, TTarget>)base.Optional(value);
        }
    }
}