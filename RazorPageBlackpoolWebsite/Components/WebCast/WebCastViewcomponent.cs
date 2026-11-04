using Microsoft.AspNetCore.Mvc;
using RazorPageBlackpoolWebsite.Components.Extensions;

namespace RazorPageBlackpoolWebsite.Components.Webcast
{
    public class WebCastViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string? introOverride)
        {
            return View(ViewComponentExtensions.GetViewPath("WebCast"), introOverride);
        }
    }
}