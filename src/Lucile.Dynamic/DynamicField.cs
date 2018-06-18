using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic
{
    public class DynamicField : DynamicMember
    {
        public DynamicField(string memberName, Type memberType)
            : base(memberName, memberType)
        {
        }

        public FieldBuilder Field { get; private set; }

        public override void CreateDeclarations(System.Reflection.Emit.TypeBuilder typeBuilder)
        {
            this.Field = typeBuilder.DefineField(this.MemberName, this.MemberType, FieldAttributes.Private);
        }

        public override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder)
        {
        }

        public override bool MemberEquals(System.Reflection.MemberInfo member)
        {
            if (member is FieldInfo)
            {
                return member.Name == this.MemberName && TypeEquals(this.MemberType, ((FieldInfo)member).FieldType);
            }

            return false;
        }
    }
}