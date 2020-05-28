using Lucile.Data.Metadata;

namespace Lucile.Core.Data.Metadata.Expressions
{
    public class ExpressionValueAccessorFactory : IValueAccessorFactory
    {
        public IEntityValueAccessor GetAccessor(IEntityMetadata entityMetadata)
        {
            return new ExpressionEntityValueAccessor(entityMetadata);
        }
    }
}
