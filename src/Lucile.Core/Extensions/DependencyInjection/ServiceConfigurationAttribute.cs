using System;

namespace Lucile.Extensions.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ServiceConfigurationAttribute : Attribute
    {
        public ServiceConfigurationAttribute(Type configurationType)
        {
            ConfigurationType = configurationType;
        }

        public Type ConfigurationType { get; }
    }
}