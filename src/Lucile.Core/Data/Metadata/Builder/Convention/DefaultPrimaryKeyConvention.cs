using System;
using System.Linq;

namespace Lucile.Data.Metadata.Builder.Convention
{
    public class DefaultPrimaryKeyConvention : IEntityConvention
    {
        public void Apply(EntityMetadataBuilder entity)
        {
            if (!entity.PrimaryKey.Any())
            {
                var key = entity.Properties.Where(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.Equals($"{entity.Name}Id", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (key != null)
                {
                    entity.PrimaryKey.Add(key.Name);
                }
            }
        }
    }
}