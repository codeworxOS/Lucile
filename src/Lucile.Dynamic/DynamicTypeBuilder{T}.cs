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
    public class DynamicTypeBuilder<T> : DynamicTypeBuilder
        where T : class
    {
        public DynamicTypeBuilder(IEnumerable<DynamicMember> dynamicMembers = null, AssemblyBuilderFactory assemblyBuilderFactory = null)
            : base(dynamicMembers, assemblyBuilderFactory)
        {
        }

        protected override Type GetBaseType()
        {
            return typeof(T);
        }
    }
}