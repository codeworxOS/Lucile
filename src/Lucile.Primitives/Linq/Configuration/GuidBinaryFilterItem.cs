namespace Lucile.Linq.Configuration
{
    public class GuidBinaryFilterItem : RelationalFilterItem
    {
        public GuidBinaryFilterItem(ValueExpression left, ValueExpression right, RelationalCompareOperator compareOperator)
            : base(left, right, compareOperator)
        {
        }
    }
}