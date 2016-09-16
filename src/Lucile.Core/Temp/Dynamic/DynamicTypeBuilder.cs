using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Reflection;
using Codeworx.Dynamic.Convention;
using Codeworx.Dynamic.Interceptor;

namespace Codeworx.Dynamic
{
    public class DynamicTypeBuilder<T> : DynamicTypeBuilder where T : class
    {
        public DynamicTypeBuilder(IEnumerable<DynamicMember> dynamicMembers = null, AssemblyBuilderFactory assemblyBuilderFactory = null) : base(dynamicMembers, assemblyBuilderFactory) { }

        protected override Type GetBaseType()
        {
            return typeof(T);
        }
    }

    public class DynamicTypeBuilder
    {
        private ObservableCollection<DynamicMember> dynamicMembers;

        private Type generatedType;

        public Type GeneratedType
        {
            get
            {
                if (generatedType == null)
                    this.GenerateType();

                return generatedType;
            }
        }

        public void AddMember(DynamicMember member)
        {
            if (!this.dynamicMembers.Contains(member))
                this.dynamicMembers.Add(member);
        }

        public void AddInterceptor(ITypeDeclarationInterceptor interceptor)
        {
            if (!this.interceptors.Contains(interceptor))
                this.interceptors.Add(interceptor);
        }

        public ReadOnlyCollection<DynamicMember> DynamicMembers { get; private set; }

        public ReadOnlyCollection<DynamicTypeConvention> Conventions { get; private set; }

        private List<DynamicTypeConvention> conventions;

        public ReadOnlyCollection<ITypeDeclarationInterceptor> Interceptors { get; private set; }

        private List<ITypeDeclarationInterceptor> interceptors;

        public AssemblyBuilderFactory AssemblyBuilderFactory { get; private set; }

        public Type BaseType { get { return GetBaseType(); } }

        protected virtual Type GetBaseType()
        {
            return typeof(object);
        }

        public DynamicTypeBuilder(IEnumerable<DynamicMember> dynamicMembers = null, AssemblyBuilderFactory assemblyBuilderFactory = null)
        {
            this.dynamicMembers = new ObservableCollection<DynamicMember>();
            this.dynamicMembers.CollectionChanged += dynamicMembers_CollectionChanged;
            this.DynamicMembers = new ReadOnlyCollection<DynamicMember>(this.dynamicMembers);

            this.conventions = new List<DynamicTypeConvention>();
            this.Conventions = new ReadOnlyCollection<DynamicTypeConvention>(this.conventions);

            this.interceptors = new List<ITypeDeclarationInterceptor>();
            this.Interceptors = new ReadOnlyCollection<ITypeDeclarationInterceptor>(this.interceptors);

            if (dynamicMembers != null)
                dynamicMembers.ToList().ForEach(p => this.dynamicMembers.Add(p));

            if (assemblyBuilderFactory == null)
                this.AssemblyBuilderFactory = new DynamicAssemblyBuilderFactory();
            else
                this.AssemblyBuilderFactory = assemblyBuilderFactory;

            AddDefaultConventions();

            //this.GenerateType();
        }

        private void dynamicMembers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null) {
                e.NewItems.OfType<DynamicMember>().ToList().ForEach(p => p.DynamicTypeBuilder = this);
            }
            if (e.OldItems != null) {
                e.OldItems.OfType<DynamicMember>().ToList().ForEach(p => p.DynamicTypeBuilder = null);
            }
        }

        private void AddDefaultConventions()
        {
            this.conventions.Add(new GetSetValueConvention());
        }

        public AssemblyBuilder AssemblyBuilder { get; private set; }

        private void GenerateType()
        {
            Type baseType = GetBaseType();

            this.AssemblyBuilder = this.AssemblyBuilderFactory.GetAssemblyBuilder();
            
#if(!SILVERLIGHT && DEBUGDYNAMIC)
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

            var typeBuilder = dynamicModule.DefineType(this.AssemblyBuilderFactory.GetUniqueTypeName(baseType),
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                baseType);
            typeBuilder.DefineDefaultConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName);


            foreach (var item in this.conventions) {
                item.Apply(this);
            }

            foreach (var item in this.dynamicMembers) {
                item.CreateDeclarations(typeBuilder);
            }

            foreach (var item in this.interceptors) {
                item.Intercept(this, typeBuilder);
            }

            foreach (var item in this.dynamicMembers) {
                item.Implement(this, typeBuilder);
            }

            this.generatedType = typeBuilder.CreateType();
#if(!SILVERLIGHT && DEBUGDYNAMIC)
            if (this.AssemblyBuilderFactory.CanPersist) {
                this.AssemblyBuilder.Save(string.Format("{0}.dll", an.Name));
            }
#endif
        }

        public void AddConvention(DynamicTypeConvention convention)
        {
            this.conventions.Add(convention);
        }
    }
}
