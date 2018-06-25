using System;

namespace Lucile.Dynamic.Test.DependencyInjection
{
    public class ScopedDependency
    {
        public ScopedDependency()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}