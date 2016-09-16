using System.Collections.Generic;

namespace Lucile.Linq
{
    internal class GroupJoinPair<TParent, TChild>
    {
        public IEnumerable<TChild> Children { get; set; }

        public TParent Parent { get; set; }
    }
}