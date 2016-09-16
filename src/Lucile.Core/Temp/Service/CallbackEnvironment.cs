using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Codeworx.Service
{
    public class CallbackEnvironment
    {
        public const string CallContextKey = "_CallbackEnvironmentChannel_";

        public const string CallContextSessionIdKey = "_CallbackEnvironmentSessionId_";

        public static T GetCallback<T>()
        {
            var result = CallContext.LogicalGetData(CallContextKey);
            if (result is T)
                return (T)result;

            return default(T);
        }

        public static String SessionId
        {
            get
            {
                var result = CallContext.LogicalGetData(CallContextSessionIdKey) as string;

                return result;
            }
        }
    }
}
