using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Interceptor
{
    public interface IMethodBodyInterceptor
    {
        void Intercept (DynamicMember parent, MethodBuilder builder, ILGenerator generator, ref Label returnLabel);
    }
}
