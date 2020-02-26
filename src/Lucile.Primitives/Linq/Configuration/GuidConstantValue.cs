using System;

namespace Lucile.Linq.Configuration
{
    public class GuidConstantValue : ConstantValueExpression<Guid>
    {
        public GuidConstantValue(Guid value)
            : base(value)
        {
        }
    }
}