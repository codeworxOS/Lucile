using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Codeworx.Dynamic.Convention;

namespace Codeworx.Dynamic.Methods
{
    public class GetProxyTargetMethod : DynamicMethod
    {
        protected override IEnumerable<string> GetGenericArguments()
        {
            yield return "T";
        }

        public GetProxyTargetMethod()
            : base("GetProxyTarget", new GenericType("T"), false)
        {

        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            var loc = il.DeclareLocal(TypeLookup(new GenericType("T")));

            var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            //var equality = typeof(Type).GetMethod("op_Equality", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var equality = typeof(object).GetMethod("Equals", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            var parameterType = il.DeclareLocal(typeof(Type));
            il.Emit(OpCodes.Ldtoken, TypeLookup(new GenericType("T")));
            il.Emit(OpCodes.Call, getTypeFromHandle);
            il.Emit(OpCodes.Stloc, parameterType);

            Dictionary<DynamicProperty, Label> jumpList = new Dictionary<DynamicProperty, Label>();
            var returnLabel = il.DefineLabel();

            foreach (var item in config.Conventions.OfType<IProxyConvention>())
            {
                var jump = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, parameterType);
                il.Emit(OpCodes.Ldtoken, item.ProxyTarget.MemberType);
                il.Emit(OpCodes.Call, getTypeFromHandle);
                il.Emit(OpCodes.Call, equality);
                il.Emit(OpCodes.Brtrue_S, jump);
                jumpList.Add(item.ProxyTarget, jump);
            }
            il.Emit(OpCodes.Ldloca_S, loc);
            il.Emit(OpCodes.Initobj, TypeLookup(new GenericType("T")));
            il.Emit(OpCodes.Br_S, returnLabel);

            foreach (var item in jumpList) {
                il.MarkLabel(item.Value);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, item.Key.PropertyGetMethod);
                il.Emit(OpCodes.Unbox_Any, TypeLookup(new GenericType("T")));
                il.Emit(OpCodes.Stloc, loc);
                il.Emit(OpCodes.Br_S, returnLabel);
            }

            il.MarkLabel(returnLabel);
            il.Emit(OpCodes.Ldloc, loc);
            il.Emit(OpCodes.Ret);
        }
    }
}