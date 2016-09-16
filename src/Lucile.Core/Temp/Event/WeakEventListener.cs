using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Codeworx.Event
{
    public class WeakEventListener<TSource, TEventArgs> : IDisposable where TEventArgs : EventArgs
    {
        Action<TSource, Action<object, TEventArgs>> addHandlerDelegate;
        Action<TSource, Action<object, TEventArgs>> removeHandlerDelegate;

        private ConditionalWeakTable<object, List<Action<object, TEventArgs>>> weakTable;
        private List<WeakReference> handlers;

        public TSource Source { get; private set; }

        public WeakEventListener(TSource source, Action<TSource, Action<object, TEventArgs>> addHandler, Action<TSource, Action<object, TEventArgs>> removeHandler)
        {
            this.weakTable = new ConditionalWeakTable<object, List<Action<object, TEventArgs>>>();
            this.handlers = new List<WeakReference>();

            this.Source = source;
            this.addHandlerDelegate = addHandler;
            this.removeHandlerDelegate = removeHandler;

            addHandlerDelegate(this.Source, ExecuteHandler);
        }

        private void ExecuteHandler(object sender, TEventArgs args)
        {
            List<Action<object, TEventArgs>> invocationList = new List<Action<object, TEventArgs>>(this.handlers.Count);
            for (int i = this.handlers.Count - 1; i >= 0; i--) {
                var item = this.handlers[i];
                var target = item.Target;

                if (item.IsAlive) {
                    List<Action<object, TEventArgs>> delegates;
                    if (this.weakTable.TryGetValue(target, out delegates)) {
                        foreach (var d in delegates) {
                            invocationList.Insert(0, d);
                        }
                    }
                } else {
                    this.handlers.RemoveAt(i);
                }
            }

            foreach (var item in invocationList) {
                item(sender, args);
            }
        }

        public void AddHandler(Action<object, TEventArgs> handler)
        {
            var result = this.weakTable.GetValue(handler.Target, p => {
                this.handlers.Add(new WeakReference(p));
                return new List<Action<object, TEventArgs>>();
            }
                );

            result.Add(handler);
        }

        public void RemoveHandler(Action<object, EventArgs> handler)
        {
            List<Action<object, TEventArgs>> result;

            if (this.weakTable.TryGetValue(handler.Target, out result)) {
                result.Remove(handler);
            }
        }

        public void Dispose()
        {
            this.weakTable = null;
            this.handlers.Clear();
            this.handlers = null;

            this.removeHandlerDelegate(this.Source, ExecuteHandler);
        }
    }
}
