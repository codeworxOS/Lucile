using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Methods
{
    public class ProxyMethodHelper
    {
        public static void GenerateBody(ILGenerator il, MethodInfo implementationGetter, MethodInfo baseMethod) {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, implementationGetter);
            var paramCount = baseMethod.GetParameters().Count();
            for (int i = 1; i <= paramCount; i++)
            {
                il.Emit(OpCodes.Ldarg, i);
            }
            il.Emit(OpCodes.Callvirt, baseMethod);
            il.Emit(OpCodes.Ret);
        }
    }
}
