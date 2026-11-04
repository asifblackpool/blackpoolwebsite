namespace RazorPageBlackpoolWebsite.Constants.Interfaces
{
        public interface ISiteConfig
        {
            string SitePath { get; }
            string SiteController { get; }
            string ViewFolder { get; }
            string SiteName { get; }          // convenience alias for SiteController
        }
}
