using System.IO;
using System.Threading.Tasks;

namespace Lucile.AspNetCore.ClientSettings
{
    public interface IClientSettingsService
    {
        Task WriteSettingsAsync(Stream response);
    }
}
