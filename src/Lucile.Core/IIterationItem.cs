using System;

namespace Lucile
{
    public interface IIterationItem
    {
        TimeSpan Duration { get; }

        TimeSpan Offset { get; }
    }
}