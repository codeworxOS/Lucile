namespace Lucile.Reflection
{
    public static class TypeKey<TEntity>
    {
        private static readonly object _key;

        static TypeKey()
        {
            _key = new object();
        }

        public static object Key
        {
            get
            {
                return _key;
            }
        }
    }
}