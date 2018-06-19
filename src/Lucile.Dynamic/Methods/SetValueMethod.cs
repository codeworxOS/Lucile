using System.Collections.Generic;
using System.Linq;
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
            var properties = config.DynamicMembers.OfType<DynamicProperty>().Where(p => p.Property.CanWrite);

            Label endLabel = il.DefineLabel();

            if (properties.Any())
            {
                Dictionary<DynamicProperty, Label> propLabels = GetSetValueHelper.CreateMethodBody(
                    properties,
                    il,
                    ref endLabel);

                foreach (var item in propLabels)
                {
                    il.MarkLabel(item.Value);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_2);
                    if (!item.Key.MemberType.IsValueType)
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
            }
            else
            {
                il.Emit(OpCodes.Br_S, endLabel);
            }

            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}