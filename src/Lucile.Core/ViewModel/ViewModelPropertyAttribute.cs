using System;

namespace Lucile.ViewModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ViewModelPropertyAttribute : Attribute
    {
        public ViewModelPropertyAttribute(string propertyName, Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }

        public bool IsReadOnly { get; set; }

        public string PropertyName
        {
            get;
            private set;
        }

        public Type PropertyType
        {
            get;
            private set;
        }
    }
}