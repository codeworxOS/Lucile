using System;
using System.Linq;
using Lucile.Dynamic.Interceptor;

namespace Lucile.Dynamic.Convention
{
    public class ImplementInterfaceConvention : DynamicTypeConvention
    {
        public ImplementInterfaceConvention(Type interfaceType)
        {
            InterfaceType = interfaceType;
        }

        public Type InterfaceType { get; }

        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            foreach (var item in InterfaceType.GetProperties())
            {
                typeBuilder.AddMember(new DynamicInterfaceProperty(item.Name, item.PropertyType));
            }

            foreach (var item in InterfaceType.GetEvents())
            {
                typeBuilder.AddMember(new DynamicEvent(item.Name, item.EventHandlerType));
            }

            foreach (var item in InterfaceType.GetMethods().Except(InterfaceType.GetProperties().SelectMany(p => p.GetAccessors())))
            {
                if (item.ReturnType != typeof(void))
                {
                    throw new InvalidOperationException("The interface can only contain void Methods, Properties and Events!");
                }

                typeBuilder.AddMember(new DynamicVoid(item.Name, item.GetParameters().Select(p => p.ParameterType).ToArray()));
            }

            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor(InterfaceType));
        }
    }
}