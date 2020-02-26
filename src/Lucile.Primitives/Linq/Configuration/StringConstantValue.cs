namespace Lucile.Linq.Configuration
{
    public class StringConstantValue : ConstantValueExpression<string>
    {
        public StringConstantValue(string value)
            : base(value)
        {
        }
    }
}