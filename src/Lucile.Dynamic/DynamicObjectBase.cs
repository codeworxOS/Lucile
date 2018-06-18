using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace Lucile.Dynamic
{
    public abstract class DynamicObjectBase : DynamicObject, IDynamicPropertyHost
    {
        public DynamicObjectBase()
        {
            this.DelegateRegister = new ConcurrentDictionary<string, List<Delegate>>();
        }

        protected internal ConcurrentDictionary<string, List<Delegate>> DelegateRegister { get; private set; }

        public abstract object GetValue(string memberName);

        public abstract void SetValue(string memberName, object value);
    }
}