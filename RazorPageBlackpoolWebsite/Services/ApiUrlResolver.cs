using Microsoft.Extensions.Options;
using RazorPageBlackpoolWebsite.Configuration;

namespace RazorPageBlackpoolWebsite.Services
{
    public class ApiUrlResolver
    {
        private readonly BlackpoolSettings _settings;
        private readonly IHostEnvironment _env;

        public ApiUrlResolver(IOptions<BlackpoolSettings> settings, IHostEnvironment env)
        {
            _settings = settings.Value;
            _env = env;
        }

        public string Resolve(HttpRequest request)
        {
            // 1. Local dev wins
            if (_env.IsDevelopment())
             //return "ht-tps://api.blackpool.gov.uk/live/";
             return "http://localhost:86/client-api";

            // 2. Host-based detection (most reliable in Contensis)
            var host = request.Host.Host.ToLowerInvariant();

            if (host.StartsWith("preview-") || host.Contains(".preview."))
                return "https://api.blackpool.gov.uk/preview/";

            if (host == "www.blackpool.gov.uk" || host == "blackpool.gov.uk")
                return "https://api.blackpool.gov.uk/live/";

            // 3. Fall back to configured value
            return _settings.BlackpoolRootApiUrl;
        }

        public string ResolveCmsUrl()
        {
            // CMS URL is the same across environments in your case
            return _settings.CmsUrl;
        }
    }
}
