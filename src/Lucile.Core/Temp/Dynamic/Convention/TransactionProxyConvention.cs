using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Codeworx.Dynamic.Interceptor;
using Codeworx.Dynamic.Methods;

namespace Codeworx.Dynamic.Convention
{
    public class TransactionProxyConvention : DynamicTypeConvention
    {
        public class TransactionProxyProperty
        {
            public PropertyInfo Property { get; set; }

            public DynamicProperty HasMultipleValuesProperty { get; set; }

            public DynamicProperty ValuesProperty { get; set; }
        }

        public Collection<TransactionProxyProperty> TransactionProxyProperties { get; private set; }

        public DynamicField TargetsField { get; private set; }

        public CommitMethod CommitMethod { get; private set; }

        public RollbackMethod RollbackMethod { get; private set; }

        public GetTransactionProxyTargetsMethod GetTransactionProxyTargetsMethod { get; set; }

        public GetTransactionProxyTypedTargetsMethod GetTransactionProxyTypedTargetsMethod { get; set; }

        public SetTransactionProxyTargetsMethod SetTransactionProxyTargetsMethod { get; set; }

        public SetTransactionProxyTypedTargetsMethod SetTransactionProxyTypedTargetsMethod { get; set; }

        public DynamicMethod UntypedTargetsGetter { get; private set; }

        public DynamicMethod UntypedTargetsSetter { get; private set; }

        public bool NotifyPropertyChanged { get; private set; }

        public TransactionProxyConvention(bool propertyChangedNotifications = true)
        {
            this.NotifyPropertyChanged = propertyChangedNotifications;
        }

        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            var properties = from p in typeBuilder.BaseType.GetProperties()
                             let set = p.GetSetMethod(true)
                             where set != null && (set.IsPublic || set.IsFamily)
                             select p;

            if (NotifyPropertyChanged) {
                foreach (var item in properties) {
                    typeBuilder.AddMember(new DynamicProperty(item.Name, item.PropertyType));
                }
            }

            this.TransactionProxyProperties = new Collection<TransactionProxyProperty>();

            foreach (var item in properties) {
                var tpp = new TransactionProxyProperty { Property = item };
                tpp.HasMultipleValuesProperty = new DynamicProperty(
                    string.Format("{0}_HasMultipleValues", item.Name),
                    typeof(bool),
                    false);

                tpp.ValuesProperty = new DynamicProperty(
                    string.Format("{0}_Values", item.Name),
                    typeof(Dictionary<,>).MakeGenericType(typeof(Key<>).MakeGenericType(item.PropertyType), typeof(List<>).MakeGenericType(typeBuilder.BaseType)),
                    true);
                typeBuilder.AddMember(tpp.HasMultipleValuesProperty);
                typeBuilder.AddMember(tpp.ValuesProperty);
                this.TransactionProxyProperties.Add(tpp);
            }

            this.TargetsField = new DynamicField(Guid.NewGuid().ToString(), typeof(List<>).MakeGenericType(typeBuilder.BaseType));
            typeBuilder.AddMember(this.TargetsField);

            this.CommitMethod = new CommitMethod();
            typeBuilder.AddMember(this.CommitMethod);
            this.RollbackMethod = new RollbackMethod();
            typeBuilder.AddMember(this.RollbackMethod);
            this.GetTransactionProxyTargetsMethod = new Methods.GetTransactionProxyTargetsMethod();
            typeBuilder.AddMember(this.GetTransactionProxyTargetsMethod);
            this.GetTransactionProxyTypedTargetsMethod = new Methods.GetTransactionProxyTypedTargetsMethod(typeBuilder.BaseType);
            typeBuilder.AddMember(this.GetTransactionProxyTypedTargetsMethod);
            this.SetTransactionProxyTargetsMethod = new Methods.SetTransactionProxyTargetsMethod();
            typeBuilder.AddMember(this.SetTransactionProxyTargetsMethod);
            this.SetTransactionProxyTypedTargetsMethod = new Methods.SetTransactionProxyTypedTargetsMethod(typeBuilder.BaseType);
            typeBuilder.AddMember(this.SetTransactionProxyTypedTargetsMethod);

            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<ICommitable>());
            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<ITransactionProxy>());
            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor(typeof(ITransactionProxy<>).MakeGenericType(typeBuilder.BaseType)));
        }
    }
}
