using System;
using System.Collections.Generic;
using System.Linq;

#if !NETSTANDARD2_0

using System.Reflection;

#endif

using System.Reflection.Emit;

namespace Lucile.Dynamic.Methods
{
    internal static class GetSetValueHelper
    {
        internal static Dictionary<DynamicProperty, Label> CreateMethodBody(DynamicTypeBuilder config, ILGenerator methodIl, ref Label endLabel)
        {
            var grouped = config.DynamicMembers.OfType<DynamicProperty>().GroupBy(p => p.MemberName.GetHashCode()).ToList();

            Dictionary<DynamicProperty, Label> propLabels = new Dictionary<DynamicProperty, Label>();

            Dictionary<int, Label> jumplist = new Dictionary<int, Label>();

            grouped.ForEach(p => jumplist[p.Key] = methodIl.DefineLabel());

            var hashCode = methodIl.DeclareLocal(typeof(int));
            methodIl.Emit(OpCodes.Ldarg_1);
            methodIl.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("GetHashCode"), null);
            methodIl.Emit(OpCodes.Stloc, hashCode);

            foreach (var item in jumplist)
            {
                methodIl.Emit(OpCodes.Ldloc, hashCode);
                methodIl.Emit(OpCodes.Ldc_I4, item.Key);
                methodIl.Emit(OpCodes.Beq, item.Value);
            }

            methodIl.Emit(OpCodes.Br, endLabel);

            foreach (var item in grouped)
            {
                methodIl.MarkLabel(jumplist[item.Key]);
                foreach (var prop in item.Select(p => p))
                {
                    var label = methodIl.DefineLabel();
                    propLabels.Add(prop, label);
                    methodIl.Emit(OpCodes.Ldarg_1);
                    methodIl.Emit(OpCodes.Ldstr, prop.MemberName);
                    methodIl.Emit(OpCodes.Call, typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) }));
                    ////methodIl.Emit(OpCodes.Ldc_I4_0);
                    ////methodIl.Emit(OpCodes.Ceq);
                    methodIl.Emit(OpCodes.Brtrue, label);
                    methodIl.Emit(OpCodes.Nop);
                }

                methodIl.Emit(OpCodes.Br, endLabel);
            }

            return propLabels;
        }
    }
}