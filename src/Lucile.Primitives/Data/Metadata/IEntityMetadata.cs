using System;
using System.Collections.Generic;

namespace Lucile.Data.Metadata
{
    public interface IEntityMetadata
    {
        string Name { get; }

        Type ClrType { get; }

        IEntityMetadata BaseEntity { get; }

        IEnumerable<IScalarProperty> GetProperties(bool includeNoneClrProperties = false);

        IEnumerable<INavigationProperty> GetNavigations();
    }
}