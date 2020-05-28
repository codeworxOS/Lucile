using System;

namespace Lucile.Data.Metadata
{
    public interface IScalarProperty : IPropertyMetadata
    {
        bool IsPrimaryKey { get; }

        Type PropertyType { get; }

        object GetValue(object entity);
    }
}