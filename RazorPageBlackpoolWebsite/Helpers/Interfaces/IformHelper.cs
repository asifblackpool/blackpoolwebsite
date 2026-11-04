using Microsoft.AspNetCore.Html;

namespace RazorPageBlackpoolWebsite.Helpers.Interfaces
{
    public interface IFormHelper
    {
        IHtmlContent TagBuilder(string formType, string contentTypeId);
    }
}
