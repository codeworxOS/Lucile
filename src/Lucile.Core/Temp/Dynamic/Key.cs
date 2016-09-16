using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public struct Key<TValue>
    {
        private TValue value;

        private bool hasValue;

        public bool HasValue
        {
            get
            {
                return hasValue;
            }
        }

        public TValue Value
        {
            get
            {
                return value;
            }
        }

        public Key(TValue value)
        {
            this.hasValue = value != null;
            this.value = value;
        }

        public override bool Equals(object other)
        {
            if (!this.HasValue) {
                if (other is Key<TValue>) {
                    return !((Key<TValue>)other).HasValue;
                }
                return other == null;
            }
            if (other is Key<TValue>) {
                return this.Value.Equals(((Key<TValue>)other).Value);
            }
            return other != null && this.Value.Equals(other);
        }

        public override int GetHashCode()
        {
            if (!this.HasValue) {
                return 0;
            }

            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            if (!this.HasValue) {
                return "";
            }
            return this.Value.ToString();
        }

        public static implicit operator Key<TValue>(TValue value)
        {
            return new Key<TValue>(value);
        }

        public static explicit operator TValue(Key<TValue> value)
        {
            return value.Value;
        }
    }
}
