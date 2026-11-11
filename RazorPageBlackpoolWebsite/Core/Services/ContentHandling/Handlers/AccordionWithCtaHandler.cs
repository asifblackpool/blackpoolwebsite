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
    public class AccordionWithCtaHandler : IContentHandler
    {
        private readonly IGovUkAccordionWithCtaButtonRenderer _accordionRenderer;

        public AccordionWithCtaHandler(IGovUkAccordionWithCtaButtonRenderer accordionRenderer)
        {
            _accordionRenderer = accordionRenderer;
        }

        // Report the raw component key instead of throwing — some factories
        // dispatch via this property for a faster match.
        string IContentHandler.ContentType => ComponentKeys.ACCORDION_CONTENT_CTA;

        public bool CanHandle(string className)
        {
            return className == typeof(AccordionWithCTAContent).Name   // "AccordionWithCTAContent"
                || className == ComponentKeys.ACCORDION_CONTENT_CTA;  // "accordionWithCTAContent"
        }

        public Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            if (string.IsNullOrEmpty(item.Content))
                return Task.FromResult<IHtmlContent>(HtmlString.Empty);

            try
            {
                var content = item.Content.Trim();
                var jsonObject = JObject.Parse(content);
                var accordionArray = jsonObject[ComponentKeys.ACCORDION_CONTENT_CTA] as JArray;

                if (accordionArray == null || accordionArray.Count == 0)
                    return Task.FromResult<IHtmlContent>(HtmlString.Empty);

                var allItems = accordionArray.ToObject<List<AccordionWithCTAContent>>();
                if (allItems == null || allItems.Count == 0)
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
                    new HtmlString("<!-- Error rendering accordion with Cta button -->"));
            }
        }
    }
}