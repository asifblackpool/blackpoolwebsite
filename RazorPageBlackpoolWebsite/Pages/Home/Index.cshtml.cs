using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Core.Interfaces;
using RazorPageBlackpoolWebsite.Models;
using RazorPageBlackpoolWebsite.Services.Interfaces;

namespace RazorPageBlackpoolWebsite.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly IContentRepository _contentRepo;
        private readonly ICmsViewModelFactory _viewModelFactory;
        private readonly ISiteConfig _siteConfig;

        public IndexModel(
            IContentRepository contentRepo,
            ICmsViewModelFactory viewModelFactory,
            ISiteConfig siteConfig)
        {
            _contentRepo = contentRepo;
            _viewModelFactory = viewModelFactory;
            _siteConfig = siteConfig;
        }

        public object ViewModel { get; private set; }
        public string ViewName { get; private set; }

        public async Task OnGetAsync(string slug)
        {
            // Build the Contensis path based on slug
            string siteViewRoot = _siteConfig.ViewFolder; // "business", "your-council", etc.
            string nodePath = string.IsNullOrEmpty(slug)
                ? $"/{siteViewRoot}"           // "/business" for root
                : $"/{siteViewRoot}/{slug}";    // "/business/business-rates", etc.

            // Fetch node from repository
            var node = await _contentRepo.GetNodeByPathAsync(nodePath);
            if (node == null)
            {
                // Optionally set a flag for 404
                ViewModel = null;
                return;
            }

            // Use the factory to determine view name and view model
            (ViewName, ViewModel) = await _viewModelFactory.CreateAsync(node);
        }
    }
}