using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public abstract class DynamicMember
    {
        public DynamicMember(string memberName, Type memberType)
        {
            if (memberName == null)
            {
                throw new ArgumentNullException(nameof(memberName));
            }

            if (string.IsNullOrWhiteSpace(memberName))
            {
                throw new ArgumentOutOfRangeException(nameof(memberName), "The Argument memberName must not be null or whitespace.");
            }

            this.MemberName = memberName;
            if (memberType != null && memberType != typeof(void))
            {
                this.MemberType = memberType;
            }
        }

        public DynamicTypeBuilder DynamicTypeBuilder { get; protected internal set; }

        public string MemberName { get; private set; }

        public Type MemberType { get; private set; }

        public abstract void CreateDeclarations(TypeBuilder typeBuilder);

        public abstract void Implement(DynamicTypeBuilder config, TypeBuilder typeBuilder);

        public abstract bool MemberEquals(MemberInfo member);

        protected bool TypeEquals(Type left, Type right)
        {
            var leftVoid = left == null || left == typeof(void);
            var rightVoid = right == null || right == typeof(void);

            if (leftVoid != rightVoid)
            {
                return false;
            }

            if (leftVoid && rightVoid)
            {
                return true;
            }

            var leftGeneric = left is GenericType || left.IsGenericParameter;
            var rightGeneric = right is GenericType || right.IsGenericParameter;

            if (leftGeneric != rightGeneric)
            {
                return false;
            }

            if (leftGeneric && rightGeneric)
            {
                return left.Name == right.Name;
            }

            return left == right;
        }
    }
}