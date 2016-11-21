using System;
using System.Runtime.Serialization;

namespace Lucile.Linq.Configuration.Builder
{
    [DataContract(IsReference = true)]
    public abstract class BaseBuilder<T>
    {
        public abstract void LoadFrom(T value);

        public abstract T ToTarget();

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