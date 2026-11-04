namespace RazorPageBlackpoolWebsite.Components.Extensions
{
    public static class ViewComponentExtensions
    {
        private const string SharedComponentsPath = "~/Pages/Components";

        public static string GetViewPath(string componentName) =>
            $"{SharedComponentsPath}/{componentName}/Default.cshtml";
    }
}