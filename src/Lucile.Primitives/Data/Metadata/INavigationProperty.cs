using System.Collections.Generic;

namespace Lucile.Data.Metadata
{
    public interface INavigationProperty : IPropertyMetadata
    {
        NavigationPropertyMultiplicity Multiplicity { get; }

        INavigationProperty TargetNavigationProperty { get; }

        IEntityMetadata TargetEntity { get; }

        IEnumerable<IForeignKey> ForeignKeyProperties { get; }
    }
}