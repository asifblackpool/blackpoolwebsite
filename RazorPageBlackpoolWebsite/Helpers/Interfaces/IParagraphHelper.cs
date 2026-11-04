using Content.Modelling.Models.Canvas.Paragraphs;
using Microsoft.AspNetCore.Html;

namespace RazorPageBlackpoolWebsite.Helpers.Interfaces
{
    // IParagraphHelper.cs
    public interface IParagraphHelper
    {
   
        Task<IHtmlContent> FragmentParagraphAsync(FragmentParagraph fp);
    }
}
