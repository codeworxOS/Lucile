using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeworx.Globalization
{
    public interface IRepositoryProvider
    {
        IEnumerable<TranslationRepository> GetRepositories();
    }
}
