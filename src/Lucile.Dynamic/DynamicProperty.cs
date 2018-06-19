using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Interceptor;

namespace Lucile.Dynamic
{
    public class DynamicProperty : DynamicMember
    {
        private List<IMethodBodyInterceptor> _setInterceptors;

        public DynamicProperty(string memberName, Type memberType, bool isReadOnly = false)
            : base(memberName, memberType)
        {
            this._setInterceptors = new List<IMethodBodyInterceptor>();
            this.SetInterceptors = new ReadOnlyCollection<IMethodBodyInterceptor>(this._setInterceptors);
            this.IsReadOnly = isReadOnly;
        }

        public FieldBuilder BackingField { get; private set; }

        public bool HasBase { get; private set; }

        public virtual bool IsOverride { get; private set; }

        public bool IsReadOnly { get; private set; }

        public PropertyBuilder Property { get; private set; }

        public MethodBuilder PropertyGetMethod { get; private set; }

        public MethodBuilder PropertySetMethod { get; private set; }

        public ReadOnlyCollection<IMethodBodyInterceptor> SetInterceptors { get; private set; }

        public void AddSetInterceptor(IMethodBodyInterceptor interceptor)
        {
            this._setInterceptors.Add(interceptor);
        }

        public override void CreateDeclarations(TypeBuilder typeBuilder)
        {
            var baseProperty = typeBuilder.BaseType.GetProperty(this.MemberName);

            if (baseProperty != null && baseProperty.PropertyType != this.MemberType)
            {
                throw new MemberTypeMissmatchException(this.MemberName);
            }
            else if (baseProperty != null)
            {
                HasBase = true;
                IsOverride = baseProperty.GetAccessors().All(p => p.IsVirtual && !p.IsFinal);
            }

            if (!HasBase)
            {
                BackingField = typeBuilder.DefineField(string.Format("val_{0}", this.MemberName), this.MemberType, FieldAttributes.Private);
            }

            MethodAttributes methodAttributes = GetMethodAttributes();

            PropertyGetMethod = typeBuilder.DefineMethod(
                                HasBase ? baseProperty.GetGetMethod().Name : string.Format("get_{0}", this.MemberName),
                                methodAttributes,
                                this.MemberType,
                                Type.EmptyTypes);

            if (!IsReadOnly)
            {
                PropertySetMethod = typeBuilder.DefineMethod(
                                    HasBase ? baseProperty.GetSetMethod().Name : string.Format("set_{0}", this.MemberName),
                                    methodAttributes,
                                    null,
                                    new[] { this.MemberType });
            }

            Property = typeBuilder.DefineProperty(this.MemberName, PropertyAttributes.HasDefault, this.MemberType, null);
            Property.SetGetMethod(PropertyGetMethod);
            if (!IsReadOnly)
            {
                Property.SetSetMethod(PropertySetMethod);
            }
        }

        public override void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder)
        {
            // GET method il
            var baseProperty = typeBuilder.BaseType.GetProperty(this.MemberName);

            var getMethodIlGenerator = PropertyGetMethod.GetILGenerator();
            if (HasBase)
            {
                getMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                getMethodIlGenerator.Emit(OpCodes.Call, baseProperty.GetGetMethod());
                getMethodIlGenerator.Emit(OpCodes.Ret);
            }
            else
            {
                getMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                getMethodIlGenerator.Emit(OpCodes.Ldfld, this.BackingField);
                getMethodIlGenerator.Emit(OpCodes.Ret);
            }

            // SET method il
            if (!IsReadOnly)
            {
                var setMethodIlGenerator = PropertySetMethod.GetILGenerator();

                Label returnLabel;
                Label originalReturn = returnLabel = setMethodIlGenerator.DefineLabel();

                foreach (var interceptor in this.SetInterceptors)
                {
                    interceptor.Intercept(this, PropertySetMethod, setMethodIlGenerator, ref returnLabel);
                }

                if (HasBase)
                {
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_1);
                    setMethodIlGenerator.Emit(OpCodes.Call, baseProperty.GetSetMethod());
                }
                else
                {
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_0);
                    setMethodIlGenerator.Emit(OpCodes.Ldarg_1);
                    setMethodIlGenerator.Emit(OpCodes.Stfld, BackingField);
                }

                setMethodIlGenerator.Emit(OpCodes.Br_S, returnLabel);

                setMethodIlGenerator.MarkLabel(originalReturn);
                setMethodIlGenerator.Emit(OpCodes.Ret);
            }
        }

        public override bool MemberEquals(MemberInfo member)
        {
            var prop = member as PropertyInfo;

            if (prop != null)
            {
                return this.MemberName == prop.Name && TypeEquals(prop.PropertyType, this.MemberType);
            }

            return false;
        }

        protected virtual MethodAttributes GetMethodAttributes()
        {
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            if (IsOverride)
            {
                methodAttributes = methodAttributes | MethodAttributes.Virtual;
            }

            return methodAttributes;
        }
    }
}