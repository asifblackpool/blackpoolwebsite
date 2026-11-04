namespace RazorPageBlackpoolWebsite.Constants
{
    public static class WebsiteConstants
    {
        // URL path (can have hyphens)
        public static readonly string SITE_PATH = "Business";

        // Controller name (no hyphens)
        public static readonly string SITE_CONTROLLER = "Business";

        // Views folder (can be whatever you want)
        public static readonly string VIEW_FOLDER = "Business";

        // For backward compatibility
        public static readonly string SITE_NAME = SITE_CONTROLLER;
        public static readonly string SITE_VIEW_PATH = SITE_PATH + "/";
        public static readonly string SHARED_COMPONENTS_PATH = "~/Pages/Components";

    }
}
