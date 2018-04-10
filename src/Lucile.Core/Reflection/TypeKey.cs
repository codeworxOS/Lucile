namespace Lucile.Reflection
{
#pragma warning disable RECS0096 // Type parameter is never used

    public static class TypeKey<TEntity>
#pragma warning restore RECS0096 // Type parameter is never used
    {
#pragma warning disable RECS0108 // Warns about static fields in generic types
        private static readonly object _key;
#pragma warning restore RECS0108 // Warns about static fields in generic types

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