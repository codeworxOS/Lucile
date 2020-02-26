namespace Lucile.Linq.Configuration
{
    public class NumericConstantValue : ConstantValueExpression<decimal>
    {
        public NumericConstantValue(decimal value)
            : base(value)
        {
        }
    }
}