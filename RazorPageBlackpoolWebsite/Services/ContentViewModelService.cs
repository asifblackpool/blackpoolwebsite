using global::RazorPageBlackpoolWebsite.ViewModels;
using RazorPageBlackpoolWebsite.Services.Interfaces;
using RazorPageBlackpoolWebsite.Models.Helpers;

namespace RazorPageBlackpoolWebsite.Services
{
    public class ContentViewModelService
    {
        private readonly IDataService<dynamic> _dataService;
        private readonly ILogger<ContentViewModelService> _logger;

        public ContentViewModelService(
            IDataService<dynamic> dataService,
            ILogger<ContentViewModelService> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public async Task<DetailsViewModel> GetViewModelForPathAsync(string path, Guid? entryId = null)
        {
            var items = await _dataService.GetAllAsync(path, entryId);

            if (items == null || !items.Any())
            {
                _logger.LogWarning("No content found for path: {Path}", path);
                return new DetailsViewModel();
            }

            return ViewModelPopulator.PopulateFromItems(items, _dataService.StatusMessage());
        }
    }
}