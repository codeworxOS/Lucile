using System.Threading.Tasks;

namespace Lucile.ViewModel
{
    public interface INavigationAware
    {
        Task CloseAsync(CloseArgs args);

        Task InitializeAsync();
    }
}