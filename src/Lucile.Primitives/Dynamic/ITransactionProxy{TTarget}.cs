using System.Collections.Generic;

namespace Lucile.Dynamic
{
    public interface ITransactionProxy<TTarget> : ICommitable
           where TTarget : class
    {
        IEnumerable<TTarget> Targets { get; }

        IEnumerable<IValueEntry<object, TTarget>> GetValueEntries(string propertyName);

        void SetTargets(IEnumerable<TTarget> targets);
    }
}