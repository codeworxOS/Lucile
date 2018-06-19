using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Lucile.Dynamic.Test.Dynamic.Test
{
    /// <summary>
    /// Base class for entities which should support validation
    /// </summary>
    [DataContract]
    public abstract class ValidationBase : TrackingBase, ISettableNotifyDataErrorInfo, IValidatable
    {
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #region Fields

        private ISettableNotifyDataErrorInfo _errorHandlingAdapter;

        #endregion Fields

        #region Properties

        public ISettableNotifyDataErrorInfo ErrorHandlingAdapter
        {
            get
            {
                return _errorHandlingAdapter;
            }
            set
            {
                if (_errorHandlingAdapter != null)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    _errorHandlingAdapter.ErrorsChanged -= this.OnErrorsChanged;
                }

                _errorHandlingAdapter = value;

                if (_errorHandlingAdapter != null)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    _errorHandlingAdapter.ErrorsChanged += this.OnErrorsChanged;
                }
            }
        }

        public bool HasErrors => this.ErrorHandlingAdapter?.HasErrors ?? false;

        public IValidatable ModelValidator { get; set; }

        #endregion Properties

        #region Private Methods

        private void OnErrorsChanged(object sender, DataErrorsChangedEventArgs dataErrorsChangedEventArgs)
        {
            this.ErrorsChanged?.Invoke(this, dataErrorsChangedEventArgs);
        }

        #endregion Private Methods

        #region Protected Methods

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            base.RaisePropertyChanged(propertyName);

            this.ModelValidator?.ValidatePropertyAsync(propertyName);
        }

        #endregion Protected Methods

        #region Public Methods

        public IEnumerable<string> ClearAllErrors()
        {
            return this.ErrorHandlingAdapter?.ClearAllErrors() ?? Enumerable.Empty<string>();
        }

        public bool ClearErrors(string propertyName)
        {
            return this.ErrorHandlingAdapter?.ClearErrors(propertyName) ?? true;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return this.ErrorHandlingAdapter?.GetErrors(propertyName) ?? Enumerable.Empty<object>();
        }

        public void SetErrors(IEnumerable<Tuple<string, IEnumerable<string>>> propertyErrors)
        {
            this.ErrorHandlingAdapter?.SetErrors(propertyErrors);
        }

        public void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            this.ErrorHandlingAdapter?.SetErrors(propertyName, errors);
        }

        public bool ValidateAll()
        {
            return this.ModelValidator?.ValidateAll() ?? true;
        }

        public Task<bool> ValidateAllAsync()
        {
            return this.ModelValidator?.ValidateAllAsync() ?? Task.FromResult(true);
        }

        public bool ValidateProperty(string propertyName)
        {
            return this.ModelValidator?.ValidateProperty(propertyName) ?? true;
        }

        public Task<bool> ValidatePropertyAsync(string propertyName)
        {
            return this.ModelValidator?.ValidatePropertyAsync(propertyName) ?? Task.FromResult(true);
        }

        #endregion Public Methods
    }
}