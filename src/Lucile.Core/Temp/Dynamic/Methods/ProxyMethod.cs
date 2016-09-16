using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Codeworx.Dynamic.Methods
{
    public class ProxyMethod : DynamicMethod
    {
        private MethodInfo baseMethod;

        private DynamicProperty implementation;

        protected override IEnumerable<string> GetGenericArguments()
        {
            foreach (var item in this.baseMethod.GetGenericArguments()) {
                yield return item.Name;
            }
        }   

        public ProxyMethod(MethodInfo info, DynamicProperty implementation) :  base(info.Name, info.ReturnType, false, info.GetParameters().Select(p => p.ParameterType).ToArray())
        {
            this.baseMethod = info;
            this.implementation = implementation;
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            ProxyMethodHelper.GenerateBody(il, this.implementation.PropertyGetMethod, this.baseMethod);
        }
    }
}
