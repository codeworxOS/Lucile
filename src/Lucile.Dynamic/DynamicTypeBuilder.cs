using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Lucile.Dynamic.Convention;
using Lucile.Dynamic.Interceptor;

namespace Lucile.Dynamic
{
    public class DynamicTypeBuilder
    {
        private List<DynamicTypeConvention> _conventions;
        private ObservableCollection<DynamicMember> _dynamicMembers;

        private Type _generatedType;

        private List<ITypeDeclarationInterceptor> _interceptors;

        public DynamicTypeBuilder(IEnumerable<DynamicMember> dynamicMembers = null, AssemblyBuilderFactory assemblyBuilderFactory = null)
        {
            this._dynamicMembers = new ObservableCollection<DynamicMember>();
            this._dynamicMembers.CollectionChanged += DynamicMembers_CollectionChanged;
            this.DynamicMembers = new ReadOnlyCollection<DynamicMember>(this._dynamicMembers);

            this._conventions = new List<DynamicTypeConvention>();
            this.Conventions = new ReadOnlyCollection<DynamicTypeConvention>(this._conventions);

            this._interceptors = new List<ITypeDeclarationInterceptor>();
            this.Interceptors = new ReadOnlyCollection<ITypeDeclarationInterceptor>(this._interceptors);

            if (dynamicMembers != null)
            {
                dynamicMembers.ToList().ForEach(p => this._dynamicMembers.Add(p));
            }

            if (assemblyBuilderFactory == null)
            {
                this.AssemblyBuilderFactory = new DynamicAssemblyBuilderFactory();
            }
            else
            {
                this.AssemblyBuilderFactory = assemblyBuilderFactory;
            }

            AddDefaultConventions();
        }

        public AssemblyBuilder AssemblyBuilder { get; private set; }

        public AssemblyBuilderFactory AssemblyBuilderFactory { get; private set; }

        public Type BaseType => GetBaseType();

        public ReadOnlyCollection<DynamicTypeConvention> Conventions { get; private set; }

        public ReadOnlyCollection<DynamicMember> DynamicMembers { get; private set; }

        public Type GeneratedType
        {
            get
            {
                if (_generatedType == null)
                {
                    this.GenerateType();
                }

                return _generatedType;
            }
        }

        public ReadOnlyCollection<ITypeDeclarationInterceptor> Interceptors { get; private set; }

        public void AddConvention(DynamicTypeConvention convention)
        {
            this._conventions.Add(convention);
        }

        public void AddInterceptor(ITypeDeclarationInterceptor interceptor)
        {
            if (!this._interceptors.Contains(interceptor))
            {
                this._interceptors.Add(interceptor);
            }
        }

        public void AddMember(DynamicMember member)
        {
            if (!this._dynamicMembers.Contains(member))
            {
                this._dynamicMembers.Add(member);
            }
        }

        protected virtual Type GetBaseType()
        {
            return typeof(object);
        }

        private void AddDefaultConventions()
        {
            this._conventions.Add(new GetSetValueConvention());
        }

        private void DynamicMembers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                e.NewItems.OfType<DynamicMember>().ToList().ForEach(p => p.DynamicTypeBuilder = this);
            }

            if (e.OldItems != null)
            {
                e.OldItems.OfType<DynamicMember>().ToList().ForEach(p => p.DynamicTypeBuilder = null);
            }
        }

        private void GenerateType()
        {
            Type baseType = GetBaseType();

            this.AssemblyBuilder = this.AssemblyBuilderFactory.GetAssemblyBuilder();

#if DEBUGDYNAMIC
            var an = this.AssemblyBuilder.GetName(false);
            ModuleBuilder dynamicModule = null;
            if (this.AssemblyBuilderFactory.CanPersist) {
                dynamicModule = AssemblyBuilder.GetDynamicModule("MainModule") ?? AssemblyBuilder.DefineDynamicModule("MainModule", string.Format("{0}.dll", an.Name));
            } else {
                dynamicModule = AssemblyBuilder.GetDynamicModule("MainModule") ?? AssemblyBuilder.DefineDynamicModule("MainModule");
            }
#else
            ModuleBuilder dynamicModule = AssemblyBuilder.GetDynamicModule("MainModule") ?? AssemblyBuilder.DefineDynamicModule("MainModule");
#endif

            var typeBuilder = dynamicModule.DefineType(
                this.AssemblyBuilderFactory.GetUniqueTypeName(baseType),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType);
            typeBuilder.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName);

            foreach (var item in this._conventions)
            {
                item.Apply(this);
            }

            foreach (var item in this._dynamicMembers)
            {
                item.CreateDeclarations(typeBuilder);
            }

            foreach (var item in this._interceptors)
            {
                item.Intercept(this, typeBuilder);
            }

            foreach (var item in this._dynamicMembers)
            {
                item.Implement(this, typeBuilder);
            }

            this._generatedType = typeBuilder.CreateTypeInfo().AsType();
#if DEBUGDYNAMIC
            if (this.AssemblyBuilderFactory.CanPersist) {
                this.AssemblyBuilder.Save(string.Format("{0}.dll", an.Name));
            }
#endif
        }
    }
}