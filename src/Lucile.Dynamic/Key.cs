namespace Lucile.Dynamic
{
    public struct Key<TValue>
    {
        private bool _hasValue;
        private TValue _value;

        public Key(TValue value)
        {
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
            this._hasValue = value != null;
#pragma warning restore RECS0017 // Possible compare of value type with 'null'
            this._value = value;
        }

        public bool HasValue
        {
            get
            {
                return _hasValue;
            }
        }

        public TValue Value
        {
            get
            {
                return _value;
            }
        }

        public static explicit operator TValue(Key<TValue> value)
        {
            return value.Value;
        }

        public static implicit operator Key<TValue>(TValue value)
        {
            return new Key<TValue>(value);
        }

        public override bool Equals(object obj)
        {
            if (!this.HasValue)
            {
                if (obj is Key<TValue>)
                {
                    return !((Key<TValue>)obj).HasValue;
                }

                return obj == null;
            }

            TValue otherValue;

            if (obj is Key<TValue>)
            {
                otherValue = ((Key<TValue>)obj).Value;
            }
            else
            {
                otherValue = (TValue)obj;
            }

            var comparer = ComparerCache.Get<TValue>();
            if (comparer != null)
            {
                return comparer.Equals(Value, otherValue);
            }

            return object.Equals(Value, otherValue);
        }

        public override int GetHashCode()
        {
            if (!this.HasValue)
            {
                return 0;
            }

            var comparer = ComparerCache.Get<TValue>();
            if (comparer != null)
            {
                return comparer.GetHashCode(Value);
            }

            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            if (!this.HasValue)
            {
                return string.Empty;
            }

            return this.Value.ToString();
        }
    }
}