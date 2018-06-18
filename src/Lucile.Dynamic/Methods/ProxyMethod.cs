using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucile.Dynamic.Methods
{
    public class ProxyMethod : DynamicMethod
    {
        private readonly MethodInfo _baseMethod;

        private readonly DynamicProperty _implementation;

        public ProxyMethod(MethodInfo info, DynamicProperty implementation)
            : base(info.Name, info.ReturnType, false, info.GetParameters().Select(p => p.ParameterType).ToArray())
        {
            this._baseMethod = info;
            this._implementation = implementation;
        }

        protected override IEnumerable<string> GetGenericArguments()
        {
            foreach (var item in this._baseMethod.GetGenericArguments())
            {
                yield return item.Name;
            }
        }

        protected override void Implement(DynamicTypeBuilder config, System.Reflection.Emit.TypeBuilder typeBuilder, ILGenerator il)
        {
            ProxyMethodHelper.GenerateBody(il, this._implementation.PropertyGetMethod, this._baseMethod);
        }
    }
}