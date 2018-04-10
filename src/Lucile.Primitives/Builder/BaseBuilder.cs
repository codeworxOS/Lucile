using System;
using System.Runtime.Serialization;

namespace Lucile.Builder
{
    [DataContract(IsReference = true)]
    public abstract class BaseBuilder<T>
    {
        public T Build()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return BuildTarget();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public abstract void LoadFrom(T value);

        [Obsolete("Use Build Method instead")]
        public T ToTarget()
        {
            return Build();
        }

        protected abstract T BuildTarget();

        protected TDerived Get<TDerived>(T value)
            where TDerived : T
        {
            if (value is TDerived)
            {
                return (TDerived)value;
            }
            else
            {
                throw new ArgumentException($"Unexpected {typeof(T)}. Expected {typeof(TDerived)} got {value?.GetType()}", nameof(value));
            }
        }
    }
}