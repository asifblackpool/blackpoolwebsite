using Content.Modelling.Models.Canvas.Panels;
using Content.Modelling.Models.GenericTypes;
using Microsoft.AspNetCore.Html;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers
{
    public class CanvasPanelComplexHandler : IContentHandler
    {
        private readonly ISerializationHelper _serializer;
        private readonly IPanelHelper _panelHelper;

        public CanvasPanelComplexHandler(ISerializationHelper serializer, IPanelHelper panelHelper)
        {
            _serializer = serializer;
            _panelHelper = panelHelper;
        }

        string IContentHandler.ContentType => throw new NotImplementedException();

        public bool CanHandle(string className) => className == typeof(CanvasPanelComplex).Name;

        public async Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            var htmlContent = new HtmlContentBuilder();

            try
            {
                // Deserialize the panel content
                var panel = await _serializer.DeserializeAsync<CanvasPanelComplex>(item);

                if (panel != null)
                {
                    // Build and append the panel HTML
                    var panelHtml = _panelHelper.BuildPanel(panel);
                    htmlContent.AppendHtml(panelHtml);
                }
            }
            catch (Exception ex)
            {
                htmlContent.AppendHtml($"<!-- Error processing CanvasPanelComplex handler: {ex.Message} -->");
            }

            return htmlContent;
        }
    }
}
