using RazorPageBlackpoolWebsite.Models;

namespace RazorPageBlackpoolWebsite.Services.Interfaces
{
  
    public interface ICmsViewModelFactory
    {
        Task<(string ViewName, object ViewModel)> CreateAsync(CmsNode node, Guid? entryId = null);
    }
}
