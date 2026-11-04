using RazorPageBlackpoolWebsite.Constants.Interfaces;

namespace RazorPageBlackpoolWebsite.Constants
{
    public class SiteConfig : ISiteConfig
    {
        public SiteConfig(IConfiguration configuration)
        {
            SitePath = configuration["SITE_PATH"] ?? throw new InvalidOperationException("SITE_PATH is not configured");
            SiteController = configuration["SITE_CONTROLLER"] ?? throw new InvalidOperationException("SITE_CONTROLLER is not configured");
            ViewFolder = configuration["VIEW_FOLDER"] ?? throw new InvalidOperationException("VIEW_FOLDER is not configured");
        }

        public string SitePath { get; }
        public string SiteController { get; }
        public string ViewFolder { get; }
        public string SiteName => SiteController;
    }
}