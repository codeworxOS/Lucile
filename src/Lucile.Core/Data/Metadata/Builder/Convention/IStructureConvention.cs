﻿using System;
using System.Collections.Generic;

namespace Lucile.Data.Metadata.Builder.Convention
{
    public interface IStructureConvention : IModelConvention
    {
        IDictionary<string, Type> GetNavigations(Type entity);

        IEnumerable<string> GetScalarProperties(Type entity);
    }
}