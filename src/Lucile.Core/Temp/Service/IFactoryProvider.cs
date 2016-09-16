using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Codeworx.Service;

namespace Codeworx.Service
{
    [InheritedExport(typeof(IFactoryProvider))]
    public interface IFactoryProvider
    {
        IServiceConnectionFactory<T> GetFactory<T>() where T : class;
    }
}
