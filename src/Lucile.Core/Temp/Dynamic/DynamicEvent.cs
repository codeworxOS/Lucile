using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Codeworx.Dynamic
{
    public class DynamicEvent : DynamicMember
    {
        public DynamicEvent(string memberName, Type memberType)
            : base(memberName, memberType)
        {
            this.IsOverride = false;
        }

        public FieldBuilder BackingField { get; private set; }

        public MethodBuilder AddMethod { get; private set; }

        public MethodBuilder RemoveMethod { get; private set; }

        public EventBuilder Event { get; private set; }

        public virtual bool IsOverride { get; private set; }

        public override void CreateDeclarations(TypeBuilder typeBuilder)
        {
            if (!IsOverride)
            {
                BackingField = typeBuilder.DefineField(string.Format("val_{0}", this.MemberName), this.MemberType, FieldAttributes.Private);
            }
            this.AddMethod = typeBuilder.DefineMethod(
                                    string.Format("add_{0}", this.MemberName),
                                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                                    null,
                                    new Type[] { this.MemberType });

            this.RemoveMethod = typeBuilder.DefineMethod(
                                    string.Format("remove_{0}", this.MemberName),
                                    MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                                    null,
                                    new Type[] { this.MemberType });


            var eventBuilder = typeBuilder.DefineEvent(this.MemberName, EventAttributes.None, this.MemberType);

            eventBuilder.SetAddOnMethod(AddMethod);
            eventBuilder.SetRemoveOnMethod(RemoveMethod);
        }

        private void GenerateHandlerMethods(FieldBuilder field, ILGenerator methodIl, bool isAdd)
        {
            methodIl.DeclareLocal(this.MemberType);
            methodIl.DeclareLocal(this.MemberType);
            methodIl.DeclareLocal(this.MemberType);

            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldfld, field);
            methodIl.Emit(OpCodes.Stloc_0);
            var loopLabel = methodIl.DefineLabel();
            methodIl.MarkLabel(loopLabel);
            methodIl.Emit(OpCodes.Ldloc_0);
            methodIl.Emit(OpCodes.Stloc_1);
            methodIl.Emit(OpCodes.Ldloc_1);
            methodIl.Emit(OpCodes.Ldarg_1);
            methodIl.Emit(OpCodes.Call,
                typeof(Delegate).GetMethod(isAdd ? "Combine" : "Remove", new Type[] { typeof(Delegate), typeof(Delegate) })
                );
            methodIl.Emit(OpCodes.Castclass, this.MemberType);
            methodIl.Emit(OpCodes.Stloc_2);
            methodIl.Emit(OpCodes.Ldarg_0);
            methodIl.Emit(OpCodes.Ldflda, field);
            methodIl.Emit(OpCodes.Ldloc_2);
            methodIl.Emit(OpCodes.Ldloc_1);
            methodIl.Emit(OpCodes.Call,
                typeof(Interlocked).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(p => p.Name == "CompareExchange" && p.IsGenericMethod)
                    .MakeGenericMethod(this.MemberType)
                );
            methodIl.Emit(OpCodes.Stloc_0);
            methodIl.Emit(OpCodes.Ldloc_0);
            methodIl.Emit(OpCodes.Ldloc_1);
            methodIl.Emit(OpCodes.Ceq);
            methodIl.Emit(OpCodes.Ldc_I4_0);
            methodIl.Emit(OpCodes.Ceq);
            methodIl.Emit(OpCodes.Brtrue_S, loopLabel);
            methodIl.Emit(OpCodes.Ret);
        }

        public override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder)
        {
            var addMethodIL = AddMethod.GetILGenerator();
            #region AddMethodIL
            GenerateHandlerMethods(BackingField, addMethodIL, true);

            #endregion

            var removeMethodIL = RemoveMethod.GetILGenerator();
            #region RemoveMethodIL
            GenerateHandlerMethods(BackingField, removeMethodIL, false);

            #endregion
        }

        public override bool MemberEquals(MemberInfo member)
        {
            var evt = member as EventInfo;
            if (evt != null)
            {
                return this.MemberName == evt.Name && this.TypeEquals(evt.EventHandlerType, this.MemberType);
            }

            return false;
        }
    }
}
