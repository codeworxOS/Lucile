using System.Linq;
using Lucile.Dynamic.Interceptor;
using Lucile.Dynamic.Methods;

namespace Lucile.Dynamic.Convention
{
    public class GetSetValueConvention : DynamicTypeConvention
    {
        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            var isDynamicObject = typeof(DynamicObjectBase).IsAssignableFrom(typeBuilder.BaseType);
            var hasPropertyHostInterface = typeof(IDynamicPropertyHost).IsAssignableFrom(typeBuilder.BaseType);

            if (typeBuilder.DynamicMembers.OfType<DynamicProperty>().Any() || isDynamicObject)
            {
                if (!typeBuilder.DynamicMembers.OfType<SetValueMethod>().Any())
                {
                    typeBuilder.AddMember(new SetValueMethod());
                }

                if (!typeBuilder.DynamicMembers.OfType<GetValueMethod>().Any())
                {
                    typeBuilder.AddMember(new GetValueMethod());
                }

                if (!hasPropertyHostInterface)
                {
                    typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<IDynamicPropertyHost>());
                }
            }
        }
    }
}