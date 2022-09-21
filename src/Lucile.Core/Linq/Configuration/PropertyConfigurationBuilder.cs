using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucile.Linq.Configuration
{
    public abstract class PropertyConfigurationBuilder
    {
        public PropertyConfigurationBuilder(PropertyInfo propInfo)
        {
            PropertyInfo = propInfo;
            PropertyName = propInfo.Name;
            PropertyType = propInfo.PropertyType;
            DeclaringType = propInfo.DeclaringType;
        }

        public bool CanAggregate { get; set; }

        public bool CanFilter { get; set; }

        public bool CanSort { get; set; }

        public Func<object, LambdaExpression> CustomFilterExpression { get; set; }

        public Type DeclaringType { get; }

        public bool IsPrimaryKey { get; set; }

        public Func<string> Label { get; set; }

        public abstract LambdaExpression MappedExpression { get; set; }

        public PropertyInfo PropertyInfo { get; }

        public string PropertyName { get; }

        public Type PropertyType { get; }

        public abstract PropertyConfigurationBuilder Property(PropertyInfo child);
    }
}