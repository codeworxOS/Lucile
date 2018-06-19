using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Lucile.Dynamic.Methods
{
    public class GetValueMethod : DynamicMethod
    {
        public GetValueMethod()
            : base("GetValue", typeof(object), false, typeof(string))
        {
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            Label endLabel = il.DefineLabel();

            Dictionary<DynamicProperty, Label> propLabels = GetSetValueHelper.CreateMethodBody(config.DynamicMembers.OfType<DynamicProperty>(), il, ref endLabel);

            foreach (var item in propLabels)
            {
                il.MarkLabel(item.Value);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, item.Key.Property.GetGetMethod(), null);
                if (item.Key.MemberType.IsValueType)
                {
                    il.Emit(OpCodes.Box, item.Key.MemberType);
                }

                il.Emit(OpCodes.Ret);
            }

            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
        }
    }
}