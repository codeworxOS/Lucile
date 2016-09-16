using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Codeworx.Dynamic.Interceptor;

namespace Codeworx.Dynamic.Convention
{
    public class NotifyPropertyChangedConvention : DynamicTypeConvention
    {
        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            if (!typeof(INotifyPropertyChanged).IsAssignableFrom(typeBuilder.BaseType))
            {

                if (!typeBuilder.Interceptors.OfType<ImplementInterfaceInterceptor<INotifyPropertyChanged>>().Any())
                {
                    typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<INotifyPropertyChanged>());
                }
                if (typeBuilder.BaseType.GetEvent("PropertyChanged") == null && !typeBuilder.DynamicMembers.OfType<DynamicEvent>().Any(p => p.MemberName == "PropertyChanged"))
                {
                    typeBuilder.AddMember(new DynamicEvent("PropertyChanged", typeof(PropertyChangedEventHandler)));
                    typeBuilder.AddMember(new RaisePropertyChangedMethod());
                }
            }

            typeBuilder.DynamicMembers.OfType<DynamicProperty>().ToList().ForEach(p => p.AddSetInterceptor(new RaisePropertyChangedInterceptor()));
        }
    }
}
