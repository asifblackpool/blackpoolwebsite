using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Core.Interfaces;
using RazorPageBlackpoolWebsite.Services.Interfaces;

namespace RazorPageBlackpoolWebsite.Pages.Home
{
    public class DetailsModel(
        IContentRepository contentRepo,
        ICmsViewModelFactory viewModelFactory,
        ISiteConfig siteConfig) : PageModel
    {
        private readonly IContentRepository _contentRepo = contentRepo;
        private readonly ICmsViewModelFactory _viewModelFactory = viewModelFactory;
        private readonly ISiteConfig _siteConfig = siteConfig;

        public object ViewModel { get; private set; }
        public string ViewName { get; private set; }

        public async Task OnGetAsync(string slug)
        {
            // Build the Contensis path based on slug
            string siteViewRoot = _siteConfig.ViewFolder;
            string nodePath = string.IsNullOrEmpty(slug)
                ? $"/{siteViewRoot}"
                : $"/{siteViewRoot}/{slug}";

            // Fetch node from repository
            var node = await _contentRepo.GetNodeByPathAsync(nodePath);
            if (node == null)
            {
                ViewModel = null;
                return;
            }

            // Use the factory to determine view name and view model
            (ViewName, ViewModel) = await _viewModelFactory.CreateAsync(node);
        }
    }
}