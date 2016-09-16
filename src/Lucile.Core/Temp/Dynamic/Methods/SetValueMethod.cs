using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Methods
{
    public class SetValueMethod : DynamicMethod
    {
        public SetValueMethod()
            : base("SetValue", null, false, typeof(string), typeof(object))
        {

        }

        protected override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder, ILGenerator methodIl)
        {
            Label endLabel = methodIl.DefineLabel();

            Dictionary<DynamicProperty, Label> propLabels = GetSetValueHelper.CreateMethodBody(config, methodIl, ref endLabel);

            foreach (var item in propLabels)
            {
                methodIl.MarkLabel(item.Value);
                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.Emit(OpCodes.Ldarg_2);
                if (!item.Key.MemberType.IsValueType)
                    methodIl.Emit(OpCodes.Castclass, item.Key.MemberType);
                else
                {
                    methodIl.Emit(OpCodes.Unbox_Any, item.Key.MemberType);
                }

                methodIl.EmitCall(OpCodes.Callvirt, item.Key.Property.GetSetMethod(),null);
                methodIl.Emit(OpCodes.Nop);
                methodIl.Emit(OpCodes.Br, endLabel);
            }

            methodIl.MarkLabel(endLabel);
            methodIl.Emit(OpCodes.Ret);
        }
    }
}