namespace Lucile.Dynamic.Convention
{
    public class ProxyConvention<T>
        : ProxyConvention
        where T : class
    {
        public ProxyConvention()
            : base(typeof(T))
        {
        }
    }
}