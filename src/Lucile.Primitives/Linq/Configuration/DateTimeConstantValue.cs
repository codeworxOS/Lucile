using System;

namespace Lucile.Linq.Configuration
{
    public class DateTimeConstantValue : ConstantValueExpression<DateTime>
    {
        public DateTimeConstantValue(DateTime value)
            : base(value)
        {
        }
    }
}