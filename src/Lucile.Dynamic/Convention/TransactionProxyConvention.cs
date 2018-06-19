using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Lucile.Dynamic.Interceptor;
using Lucile.Dynamic.Methods;

namespace Lucile.Dynamic.Convention
{
    public class TransactionProxyConvention : DynamicTypeConvention
    {
        public TransactionProxyConvention(Type itemType, bool propertyChangedNotifications = true)
        {
            this.ItemType = itemType;
            this.NotifyPropertyChanged = propertyChangedNotifications;
            this.IsInterface = itemType.IsInterface;
        }

        public CommitMethod CommitMethod { get; private set; }

        public GetTransactionProxyTargetsMethod GetTransactionProxyTargetsMethod { get; set; }

        public GetTransactionProxyTypedTargetsMethod GetTransactionProxyTypedTargetsMethod { get; set; }

        public GetValueEntriesMethod GetValueEntiresMethod { get; private set; }

        public GetValueEntriesTypedMethod GetValueEntiresTypedMethod { get; private set; }

        public HasMultipleValuesMethod HasMultipleValuesMethod { get; private set; }

        public bool IsInterface { get; }

        public Type ItemType { get; }

        public bool NotifyPropertyChanged { get; private set; }

        public RollbackMethod RollbackMethod { get; private set; }

        public SetTransactionProxyTargetsMethod SetTransactionProxyTargetsMethod { get; set; }

        public SetTransactionProxyTypedTargetsMethod SetTransactionProxyTypedTargetsMethod { get; set; }

        public DynamicField TargetsField { get; private set; }

        public Collection<TransactionProxyProperty> TransactionProxyProperties { get; private set; }

        public DynamicMethod UntypedTargetsGetter { get; private set; }

        public DynamicMethod UntypedTargetsSetter { get; private set; }

        public override void Apply(DynamicTypeBuilder typeBuilder)
        {
            var properties = from p in ItemType.GetProperties()
                             let set = p.GetSetMethod(true)
                             where set != null && (set.IsPublic || set.IsFamily)
                             select p;

            this.TransactionProxyProperties = new Collection<TransactionProxyProperty>();

            var baseTypeNotifications = !ItemType.IsInterface && ItemType.GetInterfaces().Contains(typeof(INotifyPropertyChanged));

            foreach (var item in properties)
            {
                DynamicProperty baseProp = null;

                if (IsInterface)
                {
                    baseProp = new DynamicInterfaceProperty(item.Name, item.PropertyType);
                }
                else
                {
                    baseProp = new DynamicProperty(item.Name, item.PropertyType);
                }

                var tpp = new TransactionProxyProperty { Property = item, DynamicProperty = baseProp };

                tpp.HasMultipleValuesProperty = new DynamicProperty(
                    $"{item.Name}_HasMultipleValues",
                    typeof(bool),
                    false);

                tpp.ValuesProperty = new DynamicProperty(
                    $"{item.Name}_Values",
                    typeof(Dictionary<,>).MakeGenericType(typeof(Key<>).MakeGenericType(item.PropertyType), typeof(List<>).MakeGenericType(ItemType)),
                    true);

                typeBuilder.AddMember(baseProp);
                typeBuilder.AddMember(tpp.HasMultipleValuesProperty);
                typeBuilder.AddMember(tpp.ValuesProperty);

                if (NotifyPropertyChanged)
                {
                    tpp.DynamicProperty.AddSetInterceptor(new ResetHasMultipleValuesInterceptor());
                }

                this.TransactionProxyProperties.Add(tpp);
            }

            this.TargetsField = new DynamicField(Guid.NewGuid().ToString(), typeof(List<>).MakeGenericType(ItemType));
            typeBuilder.AddMember(this.TargetsField);

            this.CommitMethod = new CommitMethod();
            typeBuilder.AddMember(this.CommitMethod);
            this.RollbackMethod = new RollbackMethod();
            typeBuilder.AddMember(this.RollbackMethod);
            this.GetTransactionProxyTargetsMethod = new Methods.GetTransactionProxyTargetsMethod();
            typeBuilder.AddMember(this.GetTransactionProxyTargetsMethod);
            this.GetTransactionProxyTypedTargetsMethod = new Methods.GetTransactionProxyTypedTargetsMethod(ItemType);
            typeBuilder.AddMember(this.GetTransactionProxyTypedTargetsMethod);
            this.SetTransactionProxyTargetsMethod = new Methods.SetTransactionProxyTargetsMethod();
            typeBuilder.AddMember(this.SetTransactionProxyTargetsMethod);
            this.SetTransactionProxyTypedTargetsMethod = new Methods.SetTransactionProxyTypedTargetsMethod(ItemType);
            typeBuilder.AddMember(this.SetTransactionProxyTypedTargetsMethod);

            this.GetValueEntiresMethod = new Methods.GetValueEntriesMethod();
            typeBuilder.AddMember(this.GetValueEntiresMethod);

            this.GetValueEntiresTypedMethod = new Methods.GetValueEntriesTypedMethod(ItemType);
            typeBuilder.AddMember(this.GetValueEntiresTypedMethod);

            this.HasMultipleValuesMethod = new Methods.HasMultipleValuesMethod();
            typeBuilder.AddMember(this.HasMultipleValuesMethod);

            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<ICommitable>());
            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor<ITransactionProxy>());
            typeBuilder.AddInterceptor(new ImplementInterfaceInterceptor(typeof(ITransactionProxy<>).MakeGenericType(ItemType)));
        }

        public class TransactionProxyProperty
        {
            public DynamicProperty DynamicProperty { get; set; }

            public DynamicProperty HasMultipleValuesProperty { get; set; }

            public PropertyInfo Property { get; set; }

            public DynamicProperty ValuesProperty { get; set; }
        }
    }
}