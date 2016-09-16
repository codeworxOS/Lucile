using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Core.ViewModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ViewModelPropertyAttribute : Attribute
    {   public string PropertyName
        {
            get;
            private set;
        }

        public Type PropertyType { 
            get;
            private set;
        }

        public bool IsReadOnly { get; set; }

        public ViewModelPropertyAttribute(string propertyName,Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }

    }
}
