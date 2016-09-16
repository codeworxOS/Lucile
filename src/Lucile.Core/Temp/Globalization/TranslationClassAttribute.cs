using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Codeworx.Globalization
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TranslationClassAttribute : Attribute, ITranslationClass
    {
        public string Locale { get; set; }

        public int Priority { get; set; }

        public TranslationClassAttribute(string locale, int priority)
        {
            this.Locale = locale;
            this.Priority = priority;
        }
    }
}
