using System.Linq;
using System.Reflection.Emit;

namespace Lucile.Dynamic.Interceptor
{
    public class ResetHasMultipleValuesInterceptor : IMethodBodyInterceptor
    {
        public void Intercept(DynamicMember parent, System.Reflection.Emit.MethodBuilder builder, System.Reflection.Emit.ILGenerator generator, ref Label returnLabel)
        {
            var prop = parent as DynamicProperty;

            var hasMultipleValuesProperty = prop.DynamicTypeBuilder.DynamicMembers.OfType<DynamicProperty>().FirstOrDefault(p => p.MemberName == $"{prop.MemberName}_HasMultipleValues");
            if (hasMultipleValuesProperty == null)
            {
                return;
            }

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4_0);
            generator.Emit(OpCodes.Call, hasMultipleValuesProperty.PropertySetMethod);
            generator.Emit(OpCodes.Nop);
        }
    }
}