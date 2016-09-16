using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Codeworx.ComponentModel
{
    public abstract class CompositionContext
    {
        public event EventHandler<UnhandledExceptionEventArgs> LoadExceptionOccured;

        protected virtual void OnLoadExceptionOccured(Exception ex, bool isTerminating)
        {
            if (LoadExceptionOccured != null) {
                LoadExceptionOccured(this, new UnhandledExceptionEventArgs(ex, isTerminating));
            }
        }

#if(!SILVERLIGHT)
        protected static Assembly LoadAssembly(string codeBase)
        {
            AssemblyName assemblyName;

            try {
                assemblyName = AssemblyName.GetAssemblyName(codeBase);
            } catch (ArgumentException) {
                assemblyName = new AssemblyName();
                assemblyName.CodeBase = codeBase;
            }

            return Assembly.Load(assemblyName);
        }
#endif

        public AssemblyValidationMode AssemblyValidationMode { get; private set; }

        public Collection<byte[]> ValidPublicKeys { get; private set; }

        public void ChangeAssemblyValidationMode(AssemblyValidationMode mode)
        {
            this.AssemblyValidationMode = mode;
        }

        protected CompositionContext()
        {
            this.AssemblyValidationMode = AssemblyValidationMode.None;
            this.ValidPublicKeys = new Collection<byte[]>();

            var supplierPublicKey = typeof(CompositionContext).Assembly.GetName().GetPublicKey();
            if (supplierPublicKey != null && supplierPublicKey.Any()) {
                this.ValidPublicKeys.Add(supplierPublicKey);
            }
        }

        protected virtual bool ShouldLoadAssembly(AssemblyName name, bool ignoreLoadErrors)
        {
            if (this.AssemblyValidationMode == AssemblyValidationMode.HasStrongName ||
                this.AssemblyValidationMode == AssemblyValidationMode.ValidateStrongName) {
                var key = name.GetPublicKey();
                if (key == null || !key.Any()) {
                    var ex = new AssemblyValidationException(string.Format("The Assembly {0} has no public key defined.", name.FullName));
                    OnLoadExceptionOccured(ex, !ignoreLoadErrors);
                    if (!ignoreLoadErrors) {
                        throw ex;
                    } else {
                        return false;
                    }
                }

                if (this.AssemblyValidationMode == AssemblyValidationMode.ValidateStrongName) {
                    if (!this.ValidPublicKeys.Any(p => p.SequenceEqual(key))) {
                        var ex = new AssemblyValidationException(string.Format("The public key of the Assembly {0} is not allowed.", name.FullName));
                        OnLoadExceptionOccured(ex, !ignoreLoadErrors);
                        if (!ignoreLoadErrors) {
                            throw ex;
                        } else {
                            return false;
                        }
                    }
                }
            }

            return !GetRegisteredAssemblies().Any(p => p.FullName == name.FullName);
        }

#if (!SILVERLIGHT)
        public virtual void AddFiles(params string[] files)
        {
            AddFiles(files, false);
        }

        public virtual void AddFiles(IEnumerable<string> files, bool ignoreErrors)
        {
            List<Assembly> assemblies = new List<Assembly>();

            foreach (var item in files) {
                try {
                    var name = AssemblyName.GetAssemblyName(item);
                    if (ShouldLoadAssembly(name, ignoreErrors)) {

                        var assembly = LoadAssembly(item);
                        assemblies.Add(assembly);
                    }
                } catch (Exception ex) {
                    OnLoadExceptionOccured(ex, !ignoreErrors);
                    if (!ignoreErrors)
                        throw ex;
                }
            }

            if (assemblies.Any()) {
                AddAssemblies(assemblies, ignoreErrors);
            }
        }

        public virtual void AddFile(string filePath)
        {
            AddFiles(new[] { filePath });
        }

#endif
        protected abstract IEnumerable<AssemblyName> GetRegisteredAssemblies();

        protected abstract void AddAssembliesInternal(IEnumerable<Assembly> assemblies);

        public void AddAssemblies(IEnumerable<Assembly> assemblies, bool ignoreErrors)
        {
            var toLoad = assemblies.Where(p => ShouldLoadAssembly(p.GetName(), ignoreErrors));
            AddAssembliesInternal(toLoad);
        }

        public void AddAssemblies(params Assembly[] assemblies)
        {
            AddAssemblies(assemblies, false);
        }

        public void AddAssembly(Assembly assembly)
        {
            AddAssemblies(new[] { assembly });
        }

        public void RemoveAssembly(Assembly assembly)
        {
            RemoveAssemblies(new[] { assembly });
        }

        public abstract void RemoveAssemblies(IEnumerable<Assembly> assemblies);

        public abstract void SatisfyImport(object component);

        public abstract void ComposeParts(object component);

        public abstract IEnumerable<T> GetExports<T>(string contractName = null);

        public abstract IEnumerable<Lazy<T, TMetadata>> GetExports<T, TMetadata>(string contractName = null);

        public abstract T GetExport<T>(string contractName = null);

        public abstract Lazy<object, object> GetExport(Type contractType);

        public abstract Lazy<T, TMetadata> GetExport<T, TMetadata>(string contractName = null);

        private static CompositionContext current;

        private static object syncRoot = new object();

        public static CompositionContext Current
        {
            get
            {
                if (current == null) {
                    lock (syncRoot) {
                        if (current == null) {
                            current = new MefCompositionContext();
                        }
                    }
                }

                return current;
            }
        }

        internal static CompositionContext Create()
        {
            return new MefCompositionContext();
        }
    }

    public class MefCompositionContext : CompositionContext
    {
        public CompositionContainer Container
        {
            get
            {
                EnsureContainer();
                return valContainer;
            }
        }

        private void EnsureContainer()
        {
            if (valContainer == null) {
                this.valContainer = new CompositionContainer(catalog, true);

                var frameworkPublicKey = typeof(string).Assembly.GetName().GetPublicKeyToken();

                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(p => !p.IsDynamic && !frameworkPublicKey.SequenceEqual(p.GetName().GetPublicKeyToken()))
                    .ToList();

                if (assemblies.Any()) {
                    AddAssemblies(assemblies, true);
                }
            }
        }

        private CompositionContainer valContainer;

        private AggregateCatalog catalog;

        public MefCompositionContext()
        {
            this.catalog = new AggregateCatalog();
        }

        public override void RemoveAssemblies(IEnumerable<Assembly> assemblies)
        {
            EnsureContainer();
            var result = new Dictionary<AggregateCatalog, List<AssemblyCatalog>>();

            FindAssemblies(assemblies, result, this.catalog);
            foreach (var item in result) {
                if (item.Key == this.catalog || this.catalog.Catalogs.Count != item.Value.Count) {
                    foreach (var ass in item.Value) {
                        item.Key.Catalogs.Remove(ass);
                    }
                } else {
                    this.catalog.Catalogs.Remove(item.Key);
                }
            }
        }

        private void FindAssemblies(IEnumerable<Assembly> assemblies, Dictionary<AggregateCatalog, List<AssemblyCatalog>> result, AggregateCatalog aggregate = null)
        {
            EnsureContainer();
            if (aggregate == null)
                aggregate = this.catalog;

            var found = (from agg in aggregate.Catalogs.OfType<AssemblyCatalog>()
                         join ip in assemblies on agg.Assembly equals ip
                         select agg).ToList();
            if (found.Any()) {
                result.Add(aggregate, found);
            }

            foreach (var item in aggregate.Catalogs.OfType<AggregateCatalog>()) {
                FindAssemblies(assemblies, result, item);
            }
        }

        private IEnumerable<Assembly> GetLoadedAssemblies(AggregateCatalog catalog)
        {
            EnsureContainer();
            foreach (var item in catalog.Catalogs.OfType<AssemblyCatalog>().Select(p => p.Assembly)) {
                yield return item;
            }

            foreach (var item in catalog.Catalogs.OfType<AggregateCatalog>()) {
                foreach (var assembly in GetLoadedAssemblies(item))
                    yield return assembly;
            }
        }

        public override void SatisfyImport(object component)
        {
            this.Container.SatisfyImportsOnce(component);
        }

        public override void ComposeParts(object component)
        {
            this.Container.ComposeParts(component);
        }

        public override IEnumerable<T> GetExports<T>(string contractName = null)
        {
            if (contractName == null)
                return this.Container.GetExports<T>().Select(p => p.Value);
            else
                return this.Container.GetExports<T>(contractName).Select(p => p.Value);

        }

        public override IEnumerable<Lazy<T, TMetadata>> GetExports<T, TMetadata>(string contractName = null)
        {
            if (contractName == null)
                return this.Container.GetExports<T, TMetadata>();
            else
                return this.Container.GetExports<T, TMetadata>(contractName);
        }

        public override T GetExport<T>(string contractName = null)
        {
            Lazy<T> lazy;

            if (contractName == null)
                lazy = this.Container.GetExport<T>();
            else
                lazy = this.Container.GetExport<T>(contractName);

            return lazy != null ? lazy.Value : default(T);
        }

        public override Lazy<object, object> GetExport(Type contractType)
        {
            return this.Container.GetExports(contractType, null, null).FirstOrDefault();
        }

        public override Lazy<T, TMetadata> GetExport<T, TMetadata>(string contractName = null)
        {
            Lazy<T, TMetadata> lazy;

            if (contractName == null)
                lazy = this.Container.GetExport<T, TMetadata>();
            else
                lazy = this.Container.GetExport<T, TMetadata>(contractName);

            return lazy;
        }

        protected override IEnumerable<AssemblyName> GetRegisteredAssemblies()
        {
            return this.GetLoadedAssemblies(this.catalog).Select(p => p.GetName());
        }

        protected override void AddAssembliesInternal(IEnumerable<Assembly> assemblies)
        {
            EnsureContainer();
            var catalogs = assemblies.Select(p => new AssemblyCatalog(p)).ToList();
            this.catalog.Catalogs.Add(new AggregateCatalog(catalogs));
        }
    }

#if(SILVERLIGHT)
    public static class AssemblyExtensions
    {
        public static AssemblyName GetName(this Assembly assembly)
        {
            return new AssemblyName(assembly.FullName);
        }
    }
#endif
}
