using System;

namespace Lucile.Reflection
{
    public class TypeResolver
    {
        public static Type GetType(string assemblyQualifiedName)
        {
            var t = Type.GetType(assemblyQualifiedName);
            //// TODO look for replacement in .net core
            ////if (t == null)
            ////{
            ////    t = Type.GetType(
            ////            assemblyQualifiedName,
            ////            p => AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name.Equals(p.Name)),
            ////            (assembly, name, throwOnError) => assembly.GetType(name, throwOnError));
            ////}
            return t;
        }
    }
}