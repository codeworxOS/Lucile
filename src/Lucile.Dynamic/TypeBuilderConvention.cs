namespace Lucile.Dynamic
{
    public abstract class TypeBuilderConvention
    {
        public virtual void OnBuilderInitialized(DynamicTypeBuilder dynamicTypeBuilder)
        {
        }

        public virtual void OnMembersCreated(DynamicTypeBuilder dynamicTypeBuilder)
        {
        }
    }
}