using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Service
{
    public class InstanceFactoryProvider : IFactoryProvider
    {
        public InstanceFactoryProvider()
        {

        }

        public IServiceConnectionFactory<T> GetFactory<T>() where T : class
        {
            return new InstanceServiceConnectionFactory<T>();
        }
    }
}
