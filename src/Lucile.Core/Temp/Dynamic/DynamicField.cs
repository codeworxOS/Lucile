using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic
{
    public class DynamicField : DynamicMember
    {
        public FieldBuilder Field { get; private set; }

        public DynamicField(string memberName, Type memberType)
            : base(memberName, memberType)
        {

        }

        public override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder)
        {

        }

        public override void CreateDeclarations(System.Reflection.Emit.TypeBuilder typeBuilder)
        {
            this.Field = typeBuilder.DefineField(this.MemberName, this.MemberType, FieldAttributes.Private);
        }

        public override bool MemberEquals(System.Reflection.MemberInfo member)
        {
            if (member is FieldInfo) {
                return member.Name == this.MemberName && TypeEquals(this.MemberType, ((FieldInfo)member).FieldType);
            }
            return false;
        }
    }
}
