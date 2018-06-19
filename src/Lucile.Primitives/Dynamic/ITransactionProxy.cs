using System.Collections.Generic;

namespace Lucile.Dynamic
{
    public interface ITransactionProxy : ICommitable
    {
        IEnumerable<object> Targets { get; }

        IEnumerable<IValueEntry<object, object>> GetValueEntries(string propertyName);

        bool HasMultipleValues(string propertyName);

        void SetTargets(IEnumerable<object> targets);
    }
}