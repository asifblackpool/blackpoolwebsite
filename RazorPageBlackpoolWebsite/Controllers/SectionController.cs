using Microsoft.AspNetCore.Mvc;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Controllers.Base;
using RazorPageBlackpoolWebsite.Services.Interfaces;

namespace RazorPageBlackpoolWebsite.Controllers
{
    public class SectionController : DynamicCmsController
    {
        private readonly ISiteConfig _siteConfig;

        protected override string ViewFolder => _siteConfig.ViewFolder;

        public SectionController(
            IZengentiClient cmsClient,
            ICmsViewModelFactory viewModelFactory,
            ILogger<SectionController> logger,
            ISiteConfig siteConfig)
            : base(cmsClient, viewModelFactory, logger)
        {
            _siteConfig = siteConfig;
        }

        public async Task<IActionResult> Index(string section, string slug)
        {
            if (string.IsNullOrEmpty(section))
                return NotFound();

            string fullSlug = string.IsNullOrEmpty(slug)
                ? section
                : $"{section}/{slug}";

            return await RenderDynamicPageAsync(ViewFolder, fullSlug);
        }
    }
}