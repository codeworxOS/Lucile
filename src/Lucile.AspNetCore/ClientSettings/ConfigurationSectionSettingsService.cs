using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Lucile.AspNetCore.ClientSettings
{
    public class ConfigurationSectionSettingsService : IClientSettingsService
    {
        private readonly IConfigurationSection _section;

        public ConfigurationSectionSettingsService(IConfigurationSection section)
        {
            _section = section;
        }

        public async Task WriteSettingsAsync(Stream response)
        {
            await using (var jw = new Utf8JsonWriter(response))
            {
                await WriteSectionAsync(jw, _section);
            }
        }

        private async Task WriteSectionAsync(Utf8JsonWriter jw, IConfigurationSection section)
        {
            jw.WriteStartObject();

            foreach (var item in section.GetChildren())
            {
                jw.WritePropertyName(item.Key);
                var children = item.GetChildren();
                if (children.Any())
                {
                    await WriteSectionAsync(jw, item).ConfigureAwait(false);
                }
                else
                {
                    jw.WriteStringValue(item.Value);
                }
            }

            jw.WriteEndObject();
        }
    }
}
