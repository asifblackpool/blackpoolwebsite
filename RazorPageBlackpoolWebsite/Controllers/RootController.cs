using Microsoft.AspNetCore.Mvc;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Controllers.Base;
using RazorPageBlackpoolWebsite.Services.Interfaces;

namespace RazorPageBlackpoolWebsite.Controllers
{
    public class RootController : DynamicCmsController
    {
        private readonly ISiteConfig _siteConfig;

        protected override string ViewFolder => _siteConfig.ViewFolder;

        public RootController(
            IZengentiClient cmsClient,
            ICmsViewModelFactory viewModelFactory,
            ILogger<RootController> logger,
            ISiteConfig siteConfig)
            : base(cmsClient, viewModelFactory, logger)
        {
            _siteConfig = siteConfig;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string slug)
        {
            slug ??= "";
            return await RenderDynamicPageAsync(_siteConfig.ViewFolder, slug);
        }
    }
}