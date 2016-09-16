using Codeworx.Core;
using Codeworx.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.Globalization
{
    public class TranslatedProperty : NotificationObject, IEquatable<TranslatedProperty>
    {
        private Func<string> valueDelegate;

        public string CategoryName { get; private set; }

        public string PropertyName { get; private set; }

        public string TranslatedText
        {
            get
            {
                try {
                    return this.valueDelegate();
                } catch {
                    return string.Format("###{0}###{1}###", CategoryName, PropertyName);
                }
            }
        }

        public TranslatedProperty(Expression<Func<string>> propertyExpression)
        {
            var member = propertyExpression.Body as MemberExpression;

            if (member != null) {
                if (typeof(TranslationRepository).IsAssignableFrom(member.Expression.Type)) {
                    try {
                        this.PropertyName = member.Member.Name;
                        this.CategoryName = Expression.Lambda<Func<string>>(Expression.Property(member.Expression, "CategoryName")).Compile()();
                    } catch { /*who cares!!! */}
                }
            }

            this.valueDelegate = propertyExpression.Compile();
            TranslationRepository.RegisterProperty(this);
        }

        public override string ToString()
        {
            return this.TranslatedText;
        }

        public bool Equals(TranslatedProperty other)
        {
            if (this.CategoryName == null && other.CategoryName == null && this.PropertyName == null && other.PropertyName == null) {
                return this.TranslatedText.Equals(other.TranslatedText);
            }
            if (this.CategoryName == null || other.CategoryName == null || this.PropertyName == null || other.PropertyName == null) {
                return false;
            }
            return this.CategoryName.Equals(other.CategoryName) && this.PropertyName.Equals(other.PropertyName);
        }

        public override bool Equals(object obj)
        {
            if (obj is TranslatedProperty)
                return this.Equals(obj as TranslatedProperty);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (CategoryName == null || PropertyName == null) {
                return TranslatedText.GetHashCode();
            }
            return CategoryName.GetHashCode() ^ PropertyName.GetHashCode();
        }

        internal void NotifyCultureChanged()
        {
            OnPropertyChanged(null);
        }
    }
}
