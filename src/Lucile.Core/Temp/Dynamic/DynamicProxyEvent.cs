using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Codeworx.Dynamic.Methods;

namespace Codeworx.Dynamic
{
    public class DynamicProxyEvent : DynamicEvent
    {
        private EventInfo baseEvent;
        private DynamicProperty implementation;

        public DynamicProxyEvent(EventInfo info, DynamicProperty implementation) : base(info.Name,info.EventHandlerType)
        {
            this.baseEvent = info;
            this.implementation = implementation;
        }

        public override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder)
        {
            var il = this.AddMethod.GetILGenerator();
            ProxyMethodHelper.GenerateBody(il, this.implementation.PropertyGetMethod, this.baseEvent.GetAddMethod());

            var il2 = this.RemoveMethod.GetILGenerator();
            ProxyMethodHelper.GenerateBody(il2, this.implementation.PropertyGetMethod, this.baseEvent.GetRemoveMethod());
        }

        public override bool IsOverride
        {
            get
            {
                return true;
            }
        }
    }
}
