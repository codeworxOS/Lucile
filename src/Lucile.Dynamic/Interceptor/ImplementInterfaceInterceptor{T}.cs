namespace Lucile.Dynamic.Interceptor
{
    public class ImplementInterfaceInterceptor<T> : ImplementInterfaceInterceptor
        where T : class
    {
        public ImplementInterfaceInterceptor()
            : base(typeof(T))
        {
        }
    }
}