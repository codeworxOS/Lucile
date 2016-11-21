using System;

namespace Lucile
{
    public static class LucileCoreExtensions
    {
        public static bool IsAlive<T>(this WeakReference<T> weak)
    where T : class
        {
            T value;
            return weak.TryGetTarget(out value);
        }

        public static T TargetOrDefault<T>(this WeakReference<T> weak)
                    where T : class
        {
            T value;
            if (weak.TryGetTarget(out value))
            {
                return value;
            }

            return default(T);
        }
    }
}