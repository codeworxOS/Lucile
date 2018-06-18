namespace Lucile.Dynamic
{
    public interface IDynamicProxy
    {
        T GetProxyTarget<T>()
            where T : class;

        void SetProxyTarget<T>(T target)
                    where T : class;
    }
}