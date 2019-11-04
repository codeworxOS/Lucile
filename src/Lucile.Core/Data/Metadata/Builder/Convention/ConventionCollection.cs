using System.Collections.ObjectModel;

namespace Lucile.Data.Metadata.Builder.Convention
{
    public class ConventionCollection : Collection<IModelConvention>
    {
        private ConventionCollection()
        {
        }

        public static ConventionCollection DefaultConventions
        {
            get
            {
                return BuildDefaultConventionCollection();
            }
        }

        private static ConventionCollection BuildDefaultConventionCollection()
        {
            var collection = new ConventionCollection();
            collection.Add(new DefaultStructureConvention());
            collection.Add(new DefaultPrimaryKeyConvention());
            collection.Add(new DefaultMaxLengthAnnotationConvention());
            return collection;
        }
    }
}