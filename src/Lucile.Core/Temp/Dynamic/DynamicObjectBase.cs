using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Collections.Concurrent;

namespace Codeworx.Dynamic
{
    [DataContract(IsReference = true)]
    public abstract class DynamicObjectBase : DynamicObject, IDynamicPropertyHost
    {
        protected internal ConcurrentDictionary<string, List<Delegate>> DelegateRegister { get; private set; }

        public DynamicObjectBase()
        {
            this.DelegateRegister = new ConcurrentDictionary<string, List<Delegate>>();
        }

        public abstract void SetValue(string memberName, object value);

        public abstract object GetValue(string memberName);
    }
}
