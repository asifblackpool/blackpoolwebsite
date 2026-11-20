using RazorPageBlackpoolWebsite.Constants.Interfaces;
using System;

namespace RazorPageBlackpoolWebsite.Services   // ← copy from your existing file
{
    public class SiteConfig : ISiteConfig
    {
        public string SitePath { get; }
        public string SiteController { get; }
        public string ViewFolder { get; }

        public string SiteName => SiteController;

        public SiteConfig()
        {
            SitePath = Environment.GetEnvironmentVariable("SITE_PATH");
            SiteController = Environment.GetEnvironmentVariable("SITE_CONTROLLER");
            ViewFolder = Environment.GetEnvironmentVariable("VIEW_FOLDER");

            if (string.IsNullOrWhiteSpace(SitePath))
                throw new InvalidOperationException("SITE_PATH is not set. Set it in .env or a launch profile.");

            if (string.IsNullOrWhiteSpace(SiteController))
                throw new InvalidOperationException("SITE_CONTROLLER is not set. Set it in .env or a launch profile.");

            if (string.IsNullOrWhiteSpace(ViewFolder))
                throw new InvalidOperationException("VIEW_FOLDER is not set. Set it in .env or a launch profile.");

            // Normalise: route patterns in Program.cs expect a leading slash.
            if (!SitePath.StartsWith("/"))
                SitePath = "/" + SitePath;
        }
    }
}