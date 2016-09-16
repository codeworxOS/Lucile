using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Codeworx.Globalization
{
    [InheritedExport(typeof(TranslationRepository))]
    public abstract class TranslationRepository<TContract> : TranslationRepository, INotifyPropertyChanged, IPartImportsSatisfiedNotification
    {
        [ImportMany]
        private List<Lazy<TContract, ITranslationClass>> implementations = new List<Lazy<TContract,ITranslationClass>>();

        protected TContract CurrentImplementation { get; private set; }

#pragma warning disable 1998
        protected async override System.Threading.Tasks.Task SetCulture(System.Globalization.CultureInfo currentCulture)
        {
            var impl = this.implementations.Where(p => new CultureInfo(p.Metadata.Locale).Equals(currentCulture)).OrderByDescending(p => p.Metadata.Priority).First();
            this.CurrentImplementation = impl.Value;
            OnPropertyChanged(null);
        }
#pragma warning restore 1998

        public override IEnumerable<System.Globalization.CultureInfo> GetAvailableCultureInfos()
        {
            return this.implementations.Select(p => new CultureInfo(p.Metadata.Locale)).Distinct().ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void OnImportsSatisfied()
        {
            ChangeCulture(Thread.CurrentThread.CurrentCulture).InvokeAsync();
        }
    }
}
