using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Methods
{
    public class GetValueMethod : DynamicMethod
    {
        public GetValueMethod()
            : base("GetValue", typeof(object), false, typeof(string))
        {

        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator methodIl)
        {
            Label endLabel = methodIl.DefineLabel();

            Dictionary<DynamicProperty, Label> propLabels = GetSetValueHelper.CreateMethodBody(config, methodIl, ref endLabel);

            foreach (var item in propLabels) {
                methodIl.MarkLabel(item.Value);
                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.EmitCall(OpCodes.Callvirt, item.Key.Property.GetGetMethod(),null);
                if (item.Key.MemberType.IsValueType) {
                    methodIl.Emit(OpCodes.Box, item.Key.MemberType);
                }
                methodIl.Emit(OpCodes.Ret);
            }

            methodIl.MarkLabel(endLabel);
            methodIl.Emit(OpCodes.Ldnull);
            methodIl.Emit(OpCodes.Ret);
        }
    }
}
