namespace Lucile.Dynamic.DependencyInjection
{
    internal static class ScopeProxyHelper
    {
        static ScopeProxyHelper()
        {
            AssemblyBuilderFactory = new StaticAssemblyBuilderFactory("Lucile.Dynamic.DependencyInjection.ScopeProxies");
        }

        internal static AssemblyBuilderFactory AssemblyBuilderFactory { get; }
    }
}