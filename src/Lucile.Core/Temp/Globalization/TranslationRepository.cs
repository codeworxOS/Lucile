using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading;
using Codeworx.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Codeworx.Globalization
{
    public abstract class TranslationRepository
    {
        private static List<WeakReference> properties;

        private static object propertiesLocker = new object();

        static TranslationRepository()
        {
            properties = new List<WeakReference>();
        }

        private static IRepositoryProvider repositoryProvider;

        public static IRepositoryProvider RepositoryProvider
        {
            get
            {
                if (repositoryProvider == null)
                    repositoryProvider = new CompositionRepositoryProvider();

                return repositoryProvider;
            }
        }

        private static CultureInfo fallbackCulture;

        public static CultureInfo FallbackCulture
        {
            get
            {
                if (fallbackCulture == null) {
                    fallbackCulture = new CultureInfo("en");
                }
                return fallbackCulture;
            }
        }

        public static void SetFallbackCulture(CultureInfo cultureInfo)
        {
            fallbackCulture = cultureInfo;
        }

        public static void SetRepositoryProvider(IRepositoryProvider provider)
        {
            repositoryProvider = provider;
        }

        public static TranslationRepository GetRepository(string category)
        {
            return RepositoryProvider.GetRepositories().FirstOrDefault(p => p.CategoryName == category);
        }

        private static CultureInfo lastSetCulture;

        public async static Task SetCurrentCulture(CultureInfo cultureInfo)
        {
            if (lastSetCulture == null || !object.Equals(lastSetCulture, cultureInfo)) {
                lastSetCulture = cultureInfo;

                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;

                var tasks = RepositoryProvider.GetRepositories().Select(p => p.ChangeCulture(cultureInfo)).ToList();
#if(SILVERLIGHT || NET4)
                await TaskEx.WhenAll(tasks);
#else
                await Task.WhenAll(tasks);
#endif
                List<TranslatedProperty> notify = new List<TranslatedProperty>();

                lock (propertiesLocker) {
                    List<WeakReference> toRemove = new List<WeakReference>();
                    foreach (var item in properties) {
                        var target = item.Target as TranslatedProperty;
                        if (item.IsAlive && target != null) {
                            notify.Add(target);
                        } else {
                            toRemove.Add(item);
                        }
                    }
                    foreach (var item in toRemove) {
                        properties.Remove(item);
                    }
                }

                foreach (var item in notify) {
                    item.NotifyCultureChanged();
                }
            }
        }

        public static IEnumerable<CultureInfo> AvailableCultures
        {
            get
            {
                foreach (var item in RepositoryProvider.GetRepositories().SelectMany(p => p.GetAvailableCultureInfos()).Distinct()) {
                    yield return item;
                }
            }
        }

        public TranslationRepository() { }

        protected abstract Task SetCulture(CultureInfo culture);

        public abstract IEnumerable<CultureInfo> GetAvailableCultureInfos();

        public abstract string CategoryName { get; }

        public async Task ChangeCulture(CultureInfo culture)
        {
            var availableCultures = this.GetAvailableCultureInfos().ToArray();
            CultureInfo current, fallback;

            fallback = availableCultures.FirstOrDefault(p => p.Equals(FallbackCulture)) ?? availableCultures.FirstOrDefault(p => p.TwoLetterISOLanguageName == FallbackCulture.TwoLetterISOLanguageName) ?? availableCultures.First();
            if (availableCultures.Contains(culture)) {
                current = culture;
            } else if (availableCultures.Any(p => p.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName)) {
                current = availableCultures.FirstOrDefault(p => p.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName);
            } else {
                current = fallback;
            }
            await SetCulture(current);
        }

        public abstract string GetResource(string resource);

        public bool TryGetResource(string resource, out string value)
        {
            try {
                value = GetResource(resource);
                return true;
            } catch { }
            value = null;
            return false;
        }

        internal static void RegisterProperty(TranslatedProperty translatedProperty)
        {
            lock (propertiesLocker) {
                bool add = true;
                var toRemove = new List<WeakReference>();
                foreach (var item in properties) {
                    if (!item.IsAlive) {
                        toRemove.Add(item);
                    } else if (item.Target == translatedProperty) {
                        add = false;
                    }
                }
                foreach (var item in toRemove) {
                    properties.Remove(item);
                }

                if (add) {
                    properties.Add(new WeakReference(translatedProperty));
                }
            }
        }


        internal static void UnregisterProperty(TranslatedProperty translatedProperty)
        {
            lock (propertiesLocker) {
                var reference = properties.Where(p => p.Target == translatedProperty).FirstOrDefault();
                if (reference != null) {
                    properties.Remove(reference);
                }
            }
        }
    }
}
