using System.Reflection;
using Lucile.Dynamic.Methods;

namespace Lucile.Dynamic
{
    public class DynamicProxyEvent : DynamicEvent
    {
        private EventInfo _baseEvent;
        private DynamicProperty _implementation;

        public DynamicProxyEvent(EventInfo info, DynamicProperty implementation)
            : base(info.Name, info.EventHandlerType)
        {
            this._baseEvent = info;
            this._implementation = implementation;
        }

        public override bool IsOverride
        {
            get
            {
                return true;
            }
        }

        public override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder)
        {
            var il = this.AddMethod.GetILGenerator();
            ProxyMethodHelper.GenerateBody(il, this._implementation.PropertyGetMethod, this._baseEvent.GetAddMethod());

            var il2 = this.RemoveMethod.GetILGenerator();
            ProxyMethodHelper.GenerateBody(il2, this._implementation.PropertyGetMethod, this._baseEvent.GetRemoveMethod());
        }
    }
}