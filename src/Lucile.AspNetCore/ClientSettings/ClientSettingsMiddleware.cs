using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace Lucile.AspNetCore.ClientSettings
{
    public class ClientSettingsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IClientSettingsService _service;

        public ClientSettingsMiddleware(RequestDelegate next, IClientSettingsService service)
        {
            _next = next;
            _service = service;
        }

        public IConfiguration Configuration { get; }

        public async Task Invoke(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            context.Response.ContentType = "application/javascript";
            context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { NoStore = true, NoCache = true };

#if NETSTANDARD2_0
            using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8, 4096, true))
#else
            await using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8, 4096, true))
#endif
            {
                var header = @"var AppSettings = (function () {
    function AppSettings()
                {
                }
                /**
                 *  Getting setting value.
                 */
                AppSettings.getValue = function(key) {
                    var segments = key.split('.');
                    var value = AppSettings.configuration;
                    for (var _i = 0, segments_1 = segments; _i < segments_1.length; _i++)
                    {
                        var path = segments_1[_i];
                        if (path in value) {
                        value = value[path];
                    }
            else
                    {
                        return undefined;
                    }
                }
                return value;
            };
            AppSettings.configuration = ";

                await writer.WriteAsync(header).ConfigureAwait(false);
            }

            await _service.WriteSettingsAsync(context.Response.Body).ConfigureAwait(false);

#if NETSTANDARD2_0
            using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8))
#else
            await using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8))
#endif
            {
                var footer = @";
                    return AppSettings;
                }
                ());";

                await writer.WriteAsync(footer).ConfigureAwait(false);
            }
        }
    }
}
