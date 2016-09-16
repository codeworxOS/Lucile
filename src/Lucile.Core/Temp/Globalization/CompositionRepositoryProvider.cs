using Codeworx.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Globalization
{
    public class CompositionRepositoryProvider : IRepositoryProvider
    {
        public IEnumerable<TranslationRepository> GetRepositories()
        {
            return CompositionContext.Current.GetExports<TranslationRepository>();
        }
    }
}
