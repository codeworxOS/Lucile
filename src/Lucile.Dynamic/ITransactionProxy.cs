using System.Collections.Generic;

namespace Lucile.Dynamic
{
    public interface ITransactionProxy : ICommitable
    {
        IEnumerable<object> Targets { get; }

        void SetTargets(IEnumerable<object> targets);
    }

    public interface ITransactionProxy<TTarget> : ICommitable
    {
        IEnumerable<TTarget> Targets { get; }

        void SetTargets(IEnumerable<TTarget> targets);
    }
}