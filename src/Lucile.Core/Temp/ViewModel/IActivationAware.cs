using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeworx.ViewModel
{
    public interface IActivationAware
    {
        Task ActivateAsync(ActivateArgs args);

        Task DeactivateAsync(DeactivateArgs args);
    }
}
