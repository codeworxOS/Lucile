using System;

namespace Lucile.Dynamic
{
    public class DynamicInterfaceProperty : DynamicProperty
    {
        public DynamicInterfaceProperty(string memberName, Type memberType, bool isReadOnly = false)
            : base(memberName, memberType, isReadOnly)
        {
        }

        public override bool IsOverride => true;
    }
}