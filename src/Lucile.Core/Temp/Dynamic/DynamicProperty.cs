using System;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Codeworx.Dynamic.Interceptor;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Codeworx.Dynamic
{
    public class DynamicProperty : DynamicMember
    {
        private List<IMethodBodyInterceptor> setInterceptors;

        public DynamicProperty(string memberName, Type memberType, bool isReadOnly = false)
            : base(memberName, memberType)
        {
            this.setInterceptors = new List<IMethodBodyInterceptor>();
            this.SetInterceptors = new ReadOnlyCollection<IMethodBodyInterceptor>(this.setInterceptors);
            this.IsReadOnly = isReadOnly;
        }

        public void AddSetInterceptor(IMethodBodyInterceptor interceptor)
        {
            this.setInterceptors.Add(interceptor);
        }

        public ReadOnlyCollection<IMethodBodyInterceptor> SetInterceptors { get; private set; }

        public bool IsReadOnly { get; private set; }

        public virtual bool IsOverride { get; private set; }

        public FieldBuilder BackingField { get; private set; }

        public MethodBuilder PropertyGetMethod { get; private set; }

        public MethodBuilder PropertySetMethod { get; private set; }

        public PropertyBuilder Property { get; private set; }

        public override void CreateDeclarations(TypeBuilder typeBuilder)
        {
            var baseProperty = typeBuilder.BaseType.GetProperty(this.MemberName);

            if (baseProperty != null && !baseProperty.GetAccessors().All(p => p.IsVirtual)) {
                throw new MemberCreationException(this.MemberName, "The property has to be declared virtual on the base class.");
            } else if (baseProperty != null && baseProperty.PropertyType != this.MemberType) {
                throw new MemberTypeMissmatchException(this.MemberName);
            } else if (baseProperty != null) {
                IsOverride = true;
            }

            if (!IsOverride) {
                BackingField = typeBuilder.DefineField(string.Format("val_{0}", this.MemberName), this.MemberType, FieldAttributes.Private);
            }

            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            if (IsOverride) {
                methodAttributes = methodAttributes | MethodAttributes.Virtual;
            }

            PropertyGetMethod = typeBuilder.DefineMethod(
                                IsOverride ? baseProperty.GetGetMethod().Name : string.Format("get_{0}", this.MemberName),
                                methodAttributes,
                                this.MemberType,
                                Type.EmptyTypes);

            if (!IsReadOnly) {
                PropertySetMethod = typeBuilder.DefineMethod(
                                    IsOverride ? baseProperty.GetSetMethod().Name : string.Format("set_{0}", this.MemberName),
                                    methodAttributes,
                                    null,
                                    new[] { this.MemberType });
            }
            Property = typeBuilder.DefineProperty(this.MemberName, PropertyAttributes.HasDefault, this.MemberType, null);
            Property.SetGetMethod(PropertyGetMethod);
            if (!IsReadOnly) {
                Property.SetSetMethod(PropertySetMethod);
            }
        }

        public override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder)
        {
            #region GetMethodILCode
            var baseProperty = typeBuilder.BaseType.GetProperty(this.MemberName);

            var getMethodIlGenerator = PropertyGetMethod.GetILGenerator();
            if (IsOverride) {
                getMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                getMethodIlGenerator.Emit(OpCodes.Call, baseProperty.GetGetMethod());
                getMethodIlGenerator.Emit(OpCodes.Ret);
            } else {
                getMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                getMethodIlGenerator.Emit(OpCodes.Ldfld, this.BackingField);
                getMethodIlGenerator.Emit(OpCodes.Ret);
            }
            #endregion

            #region SetMethodILCode
            if (!IsReadOnly) {
                var setMethodIlGenerator = PropertySetMethod.GetILGenerator();

                Label returnLabel;
                Label originalReturn = returnLabel = setMethodIlGenerator.DefineLabel();

                foreach (var interceptor in this.SetInterceptors) {
                    interceptor.Intercept(this, PropertySetMethod, setMethodIlGenerator, ref returnLabel);
                }

                if (IsOverride) {
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_1);
                    setMethodIlGenerator.Emit(OpCodes.Call, baseProperty.GetSetMethod());
                } else {
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_1);
                    setMethodIlGenerator.Emit(OpCodes.Stfld, BackingField);
                }
                setMethodIlGenerator.Emit(OpCodes.Br_S, returnLabel);

                setMethodIlGenerator.MarkLabel(originalReturn);
                setMethodIlGenerator.Emit(OpCodes.Ret);
            }
            #endregion
        }

        public override bool MemberEquals(MemberInfo member)
        {
            var prop = member as PropertyInfo;

            if (prop != null) {
                return this.MemberName == prop.Name && TypeEquals(prop.PropertyType, this.MemberType);
            }
            return false;
        }
    }
}
