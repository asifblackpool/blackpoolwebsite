using Content.Modelling.Constants;
using Content.Modelling.Models.Components;
using Content.Modelling.Models.GenericTypes;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json.Linq;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers
{
    public class AccordionContentHandler : IContentHandler
    {
        private readonly IGovUkAccordionRenderer _accordionRenderer;

        public AccordionContentHandler(IGovUkAccordionRenderer accordionRenderer)
        {
            _accordionRenderer = accordionRenderer;
        }

        string IContentHandler.ContentType => ComponentKeys.ACCORDION_CONTENT;

        public bool CanHandle(string className)
        {
            return className == typeof(AccordionContent).Name
                || className == ComponentKeys.ACCORDION_CONTENT;
        }

        public Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            if (string.IsNullOrEmpty(item.Content))
                return Task.FromResult<IHtmlContent>(HtmlString.Empty);

            try
            {
                var content = item.Content.Trim();
                var jsonObject = JObject.Parse(content);

                // The canvas helper batches consecutive accordionContent items into:
                //   { "accordionContent": [ {..}, {..}, ... ] }
                // Fall back to the single-item format in case an old-shaped payload arrives.
                List<AccordionContent> allItems;

                if (jsonObject[ComponentKeys.ACCORDION_CONTENT] is JArray batchedArray)
                {
                    allItems = batchedArray.ToObject<List<AccordionContent>>() ?? new();
                }
                else
                {
                    var single = jsonObject.ToObject<AccordionContent>();
                    allItems = single != null
                        ? new List<AccordionContent> { single }
                        : new List<AccordionContent>();
                }

                if (allItems.Count == 0)
                    return Task.FromResult<IHtmlContent>(HtmlString.Empty);

                string accordionTitle = allItems.First().Title ?? "Accordion";
                bool rememberExpanded = allItems.Count > 3;

                var options = new GovUkAccordionOptions
                {
                    AccordionId = $"accordion-{Guid.NewGuid():N}",
                    RememberExpandedState = rememberExpanded
                };

                var html = _accordionRenderer.RenderGovUkAccordion(accordionTitle, allItems, options);
                return Task.FromResult(html);
            }
            catch
            {
                return Task.FromResult<IHtmlContent>(
                    new HtmlString("<!-- Error rendering accordion content -->"));
            }
        }
    }
}