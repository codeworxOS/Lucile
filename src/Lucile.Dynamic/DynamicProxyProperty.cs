using System.Reflection;
using Lucile.Dynamic.Methods;

namespace Lucile.Dynamic
{
    public class DynamicProxyProperty : DynamicProperty
    {
        private readonly PropertyInfo _baseProperty;
        private DynamicProperty _implementation;

        public DynamicProxyProperty(PropertyInfo info, DynamicProperty implementation)
            : base(info.Name, info.PropertyType, !info.CanWrite)
        {
            this._baseProperty = info;
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
            var il = this.PropertyGetMethod.GetILGenerator();
            ProxyMethodHelper.GenerateBody(il, this._implementation.PropertyGetMethod, this._baseProperty.GetGetMethod());

            if (!this.IsReadOnly)
            {
                var il2 = this.PropertySetMethod.GetILGenerator();
                ProxyMethodHelper.GenerateBody(il2, this._implementation.PropertyGetMethod, this._baseProperty.GetSetMethod());
            }
        }
    }
}