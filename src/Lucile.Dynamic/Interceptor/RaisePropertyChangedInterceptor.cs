using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;

namespace Lucile.Dynamic.Interceptor
{
    public class RaisePropertyChangedInterceptor : IMethodBodyInterceptor
    {
        public void Intercept(DynamicMember parent, System.Reflection.Emit.MethodBuilder builder, System.Reflection.Emit.ILGenerator generator, ref Label returnLabel)
        {
            var prop = parent as DynamicProperty;
            var originalReturn = returnLabel;

            bool valueType = false;
            var compareMethod = prop.MemberType.GetMethod("op_Equality", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (compareMethod == null)
            {
                compareMethod = typeof(object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);
                valueType = true;
            }

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Ldarg_1);
            if (valueType)
            {
                generator.Emit(OpCodes.Box, prop.MemberType);
            }

            if (!prop.HasBase)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, prop.BackingField);
            }
            else
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Call, prop.PropertyGetMethod);
            }

            if (valueType)
            {
                generator.Emit(OpCodes.Box, prop.MemberType);
            }

            var endlabel = generator.DefineLabel();

            generator.Emit(OpCodes.Call, compareMethod);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Ceq);
            generator.Emit(OpCodes.Brfalse_S, returnLabel);
            generator.Emit(OpCodes.Br_S, endlabel);

            returnLabel = generator.DefineLabel();
            generator.MarkLabel(returnLabel);
            var raiseMethod = prop.DynamicTypeBuilder.DynamicMembers
                .OfType<RaisePropertyChangedMethod>()
                .Select(p => p.Method)
                .OfType<MethodInfo>()
                .FirstOrDefault();

            if (raiseMethod == null)
            {
                raiseMethod = prop.DynamicTypeBuilder.BaseType.GetMethod("RaisePropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (raiseMethod == null)
            {
                throw new MissingMemberException(builder.DeclaringType.Name, "PropertyChanged");
            }

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldstr, prop.MemberName);
            generator.Emit(OpCodes.Callvirt, raiseMethod);
            generator.Emit(OpCodes.Br_S, originalReturn);

            generator.MarkLabel(endlabel);
        }
    }
}