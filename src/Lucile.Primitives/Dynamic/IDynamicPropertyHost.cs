namespace Lucile.Dynamic
{
    public interface IDynamicPropertyHost
    {
        object GetValue(string propertyName);

        void SetValue(string propertyName, object value);
    }
}