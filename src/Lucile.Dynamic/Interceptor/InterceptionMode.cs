using System;

namespace Lucile.Dynamic.Interceptor
{
    [Flags]
    public enum InterceptionMode
    {
        Node = 0x00,
        BeforeBody = 0x01,
        AfterBody = 0x02,
        InsteadOfBody = 0x04
    }
}