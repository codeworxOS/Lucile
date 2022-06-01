#if !NETSTANDARD1_3
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Lucile
{
    public class ByteArrayComparer : EqualityComparer<byte[]>
    {
        private static readonly ByteArrayComparer _default;

        static ByteArrayComparer()
        {
            _default = new ByteArrayComparer();
        }

        public static new ByteArrayComparer Default => _default;

        public override bool Equals(byte[] x, byte[] y)
        {
            return x.Length == y.Length && memcmp(x, y, x.Length) == 0;
        }

        public override int GetHashCode(byte[] obj)
        {
            if (obj == null || obj.Length == 0)
            {
                return 0;
            }

            return obj[0].GetHashCode() ^ obj[obj.Length - 1].GetHashCode();
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
#pragma warning disable SA1300 // Element should begin with upper-case letter
        private static extern int memcmp(byte[] b1, byte[] b2, long count);
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
#endif