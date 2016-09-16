using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace Codeworx.Data
{
    [DataContract]
    public class IncludePaths
    {
        public IncludePaths()
        {
            this.Paths = new Collection<IncludePath>();
        }

        [DataMember]
        public Collection<IncludePath> Paths { get; private set; }

        public static IncludePaths Create<TEntity>(params Expression<Func<TEntity,object>>[] paths){
            var include = new IncludePaths();
            foreach (var item in paths) {
                include.Paths.Add(IncludePath.Create<TEntity>(item));
            }
            return include;
        }

        public static IncludePaths Create(params IncludePath[] paths)
        {
            var include = new IncludePaths();
            foreach (var item in paths) {
                include.Paths.Add(item);
            }
            return include;
        }
    }
}
