using Content.Modelling.Models.Canvas.Helpers;
using Content.Modelling.Models.GenericTypes;
using Content.Modelling.Models.Forms;
using Microsoft.AspNetCore.Html;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;
using RazorPageBlackpoolWebsite.Helpers.Renderers.Components;
using Content.Modelling.Constants;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers
{
    public class WebFormsHandler : IContentHandler
    {
        private readonly ViewComponentRenderer _renderer;
        private readonly ISerializationHelper _serializer;
        private readonly IFormHelper _formHelper;

        public WebFormsHandler(ViewComponentRenderer renderer,ISerializationHelper serializer, IFormHelper formHelper)
        {
            _renderer = renderer;
            _serializer = serializer;
            _formHelper = formHelper;
        }

        public string ContentType => typeof(WebForms).Name;

        public bool CanHandle(string className) => className == typeof(WebForms).Name;

        public async Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            var htmlContent = new HtmlContentBuilder();

            try
            {
                // Deserialize the web form content
                var form = await _serializer.DeserializeAsync<WebForms>(item);

                if (form?.Value?.ContentType?.Id != null)
                {
                    // Generate the form HTML
                    var formHtml = _formHelper.TagBuilder($@"lgwebsite", form.Value.ContentType.Id);
                    string temp = await _renderer.RenderAsync(ContentType, form);
                    htmlContent.AppendHtml(temp);
                }
            }
            catch (Exception ex)
            {
                htmlContent.AppendHtml($"<!-- Error processing WebForms item: {ex.Message} -->");
            }

            return htmlContent;
        }
    }
}
