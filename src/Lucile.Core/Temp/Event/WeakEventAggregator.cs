using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Codeworx.Event
{
    public class WeakEventAggregator
    {
        static WeakEventAggregator()
        {
            aggregators = new ConcurrentDictionary<object, WeakEventAggregator>();
        }

        private static ConcurrentDictionary<object, WeakEventAggregator> aggregators;

        protected static WeakEventAggregator GetOrAddAggregator(object key, Func<object, WeakEventAggregator> createDelegate)
        {
            return aggregators.GetOrAdd(key, createDelegate);
        }

        private ConditionalWeakTable<object, object> listeners;

        private object listenerLocker = new object();

        public WeakEventAggregator()
        {
            this.listeners = new ConditionalWeakTable<object, object>();
        }

        protected WeakEventListener<TSource, TEventArgs> GetListener<TSource, TEventArgs>(TSource source) where TEventArgs : EventArgs
        {
            //object value;

            //lock (listenerLocker) {
            //    if (!this.listeners.TryGetValue(source, out value)) {
            //        value = new WeakEventListener<TSource, TEventArgs>(source, addHandler, removeHandler);
            //        this.listeners.Add(source, value);
            //    }
            //}

            //return (WeakEventListener<TSource, TEventArgs>)value;
            return null;
        }
    }

    public class WeakEventAggregator<TSource, TEventArgs> : WeakEventAggregator where TEventArgs : EventArgs
    {
        private class AggregatorKey<TS, EA> : IEquatable<AggregatorKey<TS, EA>> where EA : EventArgs
        {
            public AggregatorKey(string eventName)
            {
                this.EventName = eventName;
            }

            public string EventName { get; private set; }

            public WeakEventAggregator<TS, EA> CreateAggregator()
            {
                return new WeakEventAggregator<TS, EA>(this.EventName);
            }

            public override int GetHashCode()
            {
                return (this.EventName == null ? 0 : this.EventName.GetHashCode());
            }

            public override bool Equals(object obj)
            {
                var key = obj as AggregatorKey<TSource, TEventArgs>;

                if (key != null) {
                    return this.Equals(key);
                }
                return base.Equals(obj);
            }

            #region IEquatable<AggregatorKey> Members

            public bool Equals(AggregatorKey<TS, EA> other)
            {
                return Object.Equals(this.EventName, other.EventName);
            }

            #endregion
        }

        protected static WeakEventAggregator<TSource, TEventArgs> GetOrCreateAggregator(string eventName)
        {
            return (WeakEventAggregator<TSource, TEventArgs>)GetOrAddAggregator(
                            new AggregatorKey<TSource, TEventArgs>(eventName),
                            p => {
                                return ((AggregatorKey<TSource, TEventArgs>)p).CreateAggregator();
                            });
        }

        public static void AddHandler(TSource sender, string eventName, Action<object, TEventArgs> handler)
        {
            var aggregator = GetOrCreateAggregator(eventName);
            //aggregator.GetListener(source)
        }

        public WeakEventAggregator(string eventName)
        {
            this.EventName = eventName;
        }

        public string EventName { get; private set; }
    }
}
