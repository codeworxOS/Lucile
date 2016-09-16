using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Convention
{
    public class RaisePropertyChangedMethod : DynamicMethod
    {
        public RaisePropertyChangedMethod() : base("OnPropertyChanged",null,true,typeof(string))
        {

        }
        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            var evt = config.DynamicMembers.OfType<DynamicEvent>().Where(p => p.MemberName == "PropertyChanged").FirstOrDefault();

            if (evt == null) { 
#if(SILVERLIGHT)
                throw new MissingMemberException(string.Format("Missing Member {1} on type {0}",typeBuilder.Name, "PropertyChanged"));
#else
                throw new MissingMemberException(typeBuilder.Name,"PropertyChanged");
#endif
            }
            var endLabel = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld,evt.BackingField);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, endLabel);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld,evt.BackingField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj,typeof(PropertyChangedEventArgs).GetConstructor(new []{typeof(string)}));
            il.Emit(OpCodes.Callvirt,typeof(PropertyChangedEventHandler).GetMethod("Invoke"));

            il.MarkLabel(endLabel);
            il.Emit(OpCodes.Ret);
        }
    }
}