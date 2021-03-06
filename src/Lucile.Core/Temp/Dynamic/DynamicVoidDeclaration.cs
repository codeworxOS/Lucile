﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public class DynamicVoidDeclaration : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action implementation) { base.Unsubscribe(implementation); }
    }

    public class DynamicVoidDeclaration<TArg1> : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action<TArg1> implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action<TArg1> implementation) { base.Unsubscribe(implementation); }
    }

    public class DynamicVoidDeclaration<TArg1, TArg2> : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action<TArg1, TArg2> implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action<TArg1, TArg2> implementation) { base.Unsubscribe(implementation); }
    }

    public class DynamicVoidDeclaration<TArg1, TArg2, TArg3> : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action<TArg1, TArg2, TArg3> implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action<TArg1, TArg2, TArg3> implementation) { base.Unsubscribe(implementation); }
    }

    public class DynamicVoidDeclaration<TArg1, TArg2, TArg3, TArg4> : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action<TArg1, TArg2, TArg3, TArg4> implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action<TArg1, TArg2, TArg3, TArg4> implementation) { base.Unsubscribe(implementation); }
    }

    public class DynamicVoidDeclaration<TArg1, TArg2, TArg3, TArg4, TArg5> : DynamicVoidDeclarationBase
    {
        public void Subscribe(Action<TArg1, TArg2, TArg3, TArg4, TArg5> implementation) { base.Subscribe(implementation); }
        public void Unsubscribe(Action<TArg1, TArg2, TArg3, TArg4, TArg5> implementation) { base.Unsubscribe(implementation); }
    }

    public abstract class DynamicVoidDeclarationBase
    {
        public string CallingMemberName { get; set; }

        public DynamicObjectBase DynamicObject { get; internal set; }
        
        protected void Subscribe(Delegate action){
            List<Delegate> subscriptions;
            
            if (!this.DynamicObject.DelegateRegister.TryGetValue(this.CallingMemberName,out subscriptions)){
                subscriptions = new List<Delegate>();
            }
            subscriptions.Add(action);
            this.DynamicObject.DelegateRegister.AddOrUpdate(this.CallingMemberName,subscriptions,(p,q) => subscriptions);
        }

        protected void Unsubscribe(Delegate action)
        {
            List<Delegate> subscriptions;
            if (this.DynamicObject.DelegateRegister.TryGetValue(this.CallingMemberName, out subscriptions))
            {
                if (subscriptions.Contains(action)) {
                    subscriptions.Remove(action);
                }
            }
        }
    }
}
