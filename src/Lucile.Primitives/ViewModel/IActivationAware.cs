using System.Threading.Tasks;

namespace Lucile.ViewModel
{
    public interface IActivationAware
    {
        Task ActivateAsync(ActivateArgs args);

        Task DeactivateAsync(DeactivateArgs args);
    }
}