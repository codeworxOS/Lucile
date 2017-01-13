using System;

namespace Lucile
{
    public class IterationItem<T> : IIterationItem
    {
        public IterationItem(TimeSpan offset, TimeSpan duration, T item)
        {
            this.Offset = offset;
            this.Duration = duration;
            this.Value = item;
        }

        public TimeSpan Duration
        {
            get;
            private set;
        }

        public TimeSpan Offset
        {
            get;
            private set;
        }

        public T Value
        {
            get;
            private set;
        }
    }
}