using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public class ServiceSettings
    {
        private static readonly object clientIdentityLocker = new object();

        private static string clientIdentity;

        public static string ClientIdentity
        {
            get
            {
                if (clientIdentity == null) {
                    lock (clientIdentityLocker) {
                        if (clientIdentity == null) {
                            clientIdentity = CreateServiceIdentity();
                        }
                    }
                }
                return clientIdentity;
            }
            set
            {
                lock (clientIdentityLocker) {
                    clientIdentity = value;
                }
            }
        }

        private static string CreateServiceIdentity()
        {
#if SILVERLIGHT
            return Guid.NewGuid().ToString();
#else
            var identity = Environment.MachineName;
            if (Interop.User32.GetSystemMetrics(Interop.SystemMetric.SM_REMOTESESSION) != 0) {
                identity = string.Format("{0}-#{1}", identity, System.Diagnostics.Process.GetCurrentProcess().SessionId);
            }
            return identity;
#endif
        }
    }
}
