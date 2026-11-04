using HtmlAgilityPack;

namespace RazorPageBlackpoolWebsite.Helpers.Interfaces
{
    public interface IHtmlTransformation
    {
        Task ApplyAsync(HtmlDocument document);
    }
}
