namespace Lucile.Data.Metadata.Builder.Navigation
{
    public class OneToManyNavigationBuilder<TSource, TTarget> : OneToManyNavigationBuilder
        where TSource : class
        where TTarget : class
    {
        public OneToManyNavigationBuilder(OneNavigationBuilder oneBuilder, string propertyName = null)
            : base(oneBuilder, propertyName)
        {
        }
    }
}