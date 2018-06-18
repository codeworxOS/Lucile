using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic.Methods
{
    public class SetValueMethod : DynamicMethod
    {
        public SetValueMethod()
            : base("SetValue", null, false, typeof(string), typeof(object))
        {
        }

        protected override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder, ILGenerator il)
        {
            Label endLabel = il.DefineLabel();

            Dictionary<DynamicProperty, Label> propLabels = GetSetValueHelper.CreateMethodBody(config, il, ref endLabel);

            foreach (var item in propLabels)
            {
                il.MarkLabel(item.Value);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_2);
                if (!item.Key.MemberType.GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Castclass, item.Key.MemberType);
                }
                else
                {
                    il.Emit(OpCodes.Unbox_Any, item.Key.MemberType);
                }

                il.EmitCall(OpCodes.Callvirt, item.Key.Property.GetSetMethod(), null);
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Br, endLabel);
            }

            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}