using Content.Modelling.Models.GenericTypes;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces
{
    public interface IContentHandler
    {
        string ContentType { get; }
        bool CanHandle(string className);

        Task<IHtmlContent> HandleAsync(SerialisedItem item);

        // ▼ ADD THIS — default impl keeps existing handlers untouched.
        Task<IHtmlContent> HandleAsync(SerialisedItem item, ViewContext viewContext)
            => HandleAsync(item);
    }
}
