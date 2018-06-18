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

            if (obj is Key<TValue>)
            {
                return this.Value.Equals(((Key<TValue>)obj).Value);
            }

            return obj != null && this.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (!this.HasValue)
            {
                return 0;
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