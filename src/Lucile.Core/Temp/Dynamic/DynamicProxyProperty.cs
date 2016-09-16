using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Codeworx.Dynamic.Methods;

namespace Codeworx.Dynamic
{
    public class DynamicProxyProperty : DynamicProperty
    {
        private DynamicProperty implementation;

        public PropertyInfo baseProperty { get; set; }

        public DynamicProxyProperty(PropertyInfo info, DynamicProperty implementation)
            : base(info.Name, info.PropertyType, !info.CanWrite)
        {
            this.baseProperty = info;
            this.implementation = implementation;
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
            ProxyMethodHelper.GenerateBody(il, this.implementation.PropertyGetMethod, this.baseProperty.GetGetMethod());

            if (!this.IsReadOnly) {
                var il2 = this.PropertySetMethod.GetILGenerator();
                ProxyMethodHelper.GenerateBody(il2, this.implementation.PropertyGetMethod, this.baseProperty.GetSetMethod());
            }
        }
    }
}
