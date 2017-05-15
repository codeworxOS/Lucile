namespace Lucile.Linq.Configuration
{
    public class DateTimeBinaryFilterItem : RelationalFilterItem
    {
        public DateTimeBinaryFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator compareOperator)
            : base(left, right, compareOperator)
        {
        }
    }
}