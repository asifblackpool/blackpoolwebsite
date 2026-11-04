
using Content.Modelling.Models.Interfaces;

namespace RazorPageBlackpoolWebsite.Core.Interfaces
{
    public interface IContentService
    {
        Task<List<IPageTemplates>> GetChildPagesAsync(string parentUri);
    }
}
