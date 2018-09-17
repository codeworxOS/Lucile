using System;

namespace Lucile.Data.Metadata.Builder
{
    [Flags]
    public enum AutoGenerateValue
    {
        None = 0x00,
        OnInsert = 0x01,
        OnUpdate = 0x02,
        Both = OnInsert | OnUpdate
    }
}