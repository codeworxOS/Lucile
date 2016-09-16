using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Codeworx.Dynamic;
using Codeworx.Dynamic.Convention;

namespace Codeworx.Service
{
    public class ChannelProxyFactory<TChannel, TProxy>
        where TChannel : class
        where TProxy : ChannelProxy<TChannel>
    {

        private ChannelProxyCacheItem<TChannel, TProxy> cacheItem;

        public bool HasCallback
        {
            get
            {
                return cacheItem.HasCallback;
            }
        }

        public Type CallbackType
        {
            get
            {
                return cacheItem.CallbackType;
            }
        }

        public ChannelProxyFactory()
        {
            cacheItem = ChannelProxyCache.Current.GetOrAdd<TChannel, TProxy>();
        }

        public TProxy GetProxy()
        {
            return cacheItem.GetProxy();
        }
    }
}
