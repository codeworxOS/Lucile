using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Codeworx.Dynamic
{
    public interface ICommitable
    {
        void Commit();

        void Rollback();
    }

    public interface ITransactionProxy : ICommitable
    {
        void SetTargets(IEnumerable<object> targets);

        IEnumerable<object> Targets { get; }
    }


    public interface ITransactionProxy<TTarget> : ICommitable
    {
        void SetTargets(IEnumerable<TTarget> targets);

        IEnumerable<TTarget> Targets { get; }
    }
}
