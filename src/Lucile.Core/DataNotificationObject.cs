using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Lucile
{
    public class DataNotificationObject : NotificationObject, INotifyDataErrorInfo
    {
        private ConcurrentDictionary<string, object[]> _validationErrors = new ConcurrentDictionary<string, object[]>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public virtual bool HasErrors => !_validationErrors.IsEmpty;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName))
            {
                return null;
            }

            return _validationErrors[propertyName];
        }

        protected void UpdateValidation(IEnumerable<object> validation, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var entries = validation.ToArray();

            if (entries.Length != 0)
            {
                _validationErrors.AddOrUpdate(propertyName, entries, (key, val) => entries);
            }
            else if (_validationErrors.ContainsKey(propertyName))
            {
                _validationErrors.TryRemove(propertyName, out var _);
            }

            RaiseErrorsChanged(propertyName);
        }

        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            RaisePropertyChanged(nameof(HasErrors));
        }
    }
}