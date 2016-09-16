using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Codeworx.Service
{
    public class ChannelCreationScope : IDisposable
    {
#if(SILVERLIGHT)
        [ThreadStatic]
        private static ChannelCreationScope current;
#else
        public const string CallContextKey = "Codeworx.Service.ChannelCrationScope";
#endif

        private object oldValue;

        public object Proxy { get; private set; }

        public bool DisableProxyWrapping { get; set; }

        public ChannelCreationScope(object proxy)
        {
#if(SILVERLIGHT)
            oldValue = current;
#else
            oldValue = CallContext.LogicalGetData(CallContextKey);
#endif
            this.Proxy = proxy;

#if(SILVERLIGHT)
            current = this;
#else
            CallContext.LogicalSetData(CallContextKey, this);
#endif
        }

        public static ChannelCreationScope Current
        {
            get
            {
#if(SILVERLIGHT)
                return current;
#else
                return CallContext.LogicalGetData(CallContextKey) as ChannelCreationScope;
#endif
            }
        }


        private bool _disposed;

        protected virtual bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

        ~ChannelCreationScope()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (oldValue != null)
                    {
#if(SILVERLIGHT)
                        current = (ChannelCreationScope)oldValue;
#else
                        CallContext.LogicalSetData(CallContextKey, oldValue);
#endif
                    }
                    else
                    {
#if(SILVERLIGHT)
                        current = null;
#else
                        CallContext.FreeNamedDataSlot(CallContextKey);
#endif
                    }
                }
                // Cleanup native resources here!
                _disposed = true;
            }
        }
    }
}
