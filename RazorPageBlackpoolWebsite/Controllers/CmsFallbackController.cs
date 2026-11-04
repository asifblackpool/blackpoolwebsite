using Microsoft.AspNetCore.Mvc;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Controllers.Base;
using RazorPageBlackpoolWebsite.Services.Interfaces;

namespace RazorPageBlackpoolWebsite.Controllers
{
    public class CmsFallbackController : DynamicCmsController
    {
        private readonly ISiteConfig _siteConfig;

        protected override string ViewFolder => _siteConfig.ViewFolder;

        public CmsFallbackController(
            IZengentiClient cmsClient,
            ICmsViewModelFactory viewModelFactory,
            ILogger<CmsFallbackController> logger,
            ISiteConfig siteConfig)
            : base(cmsClient, viewModelFactory, logger)
        {
            _siteConfig = siteConfig;
        }

        public async Task<IActionResult> Dynamic(string section, string slug)
        {
            return await RenderDynamicPageAsync(section, slug);
        }
    }
}