using System;
using System.ComponentModel;
using System.Linq;

#if !NETSTANDARD2_0

using System.Reflection;

#endif

using System.Reflection.Emit;

namespace Lucile.Dynamic.Convention
{
    public class RaisePropertyChangedMethod : DynamicMethod
    {
        public RaisePropertyChangedMethod()
            : base("OnPropertyChanged", null, true, typeof(string))
        {
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            var evt = config.DynamicMembers.OfType<DynamicEvent>().Where(p => p.MemberName == "PropertyChanged").FirstOrDefault();

            if (evt == null)
            {
#if NETSTANDARD1_6
                throw new MissingMemberException($"Missiong member PropertyChanged on type {typeBuilder.Name}.");
#else
                throw new MissingMemberException(typeBuilder.Name, "PropertyChanged");
#endif
            }

            var endLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, evt.BackingField);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, endLabel);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, evt.BackingField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) }));
            il.Emit(OpCodes.Callvirt, typeof(PropertyChangedEventHandler).GetMethod("Invoke"));

            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}