using Content.Modelling.Constants;
using Content.Modelling.Helpers;
using Content.Modelling.Models.Canvas.Helpers;
using Content.Modelling.Models.Canvas.Lists;
using Content.Modelling.Models.GenericTypes;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Core.Services.ContentProcessing.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Extensions;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers
{
    public class CanvasListItemHandler : IContentHandler
    {
        private readonly ISerializationHelper _serializer;
        private readonly ITextProcessor _textProcessor;
        private readonly IContentFragmentHelper _contentFragmentHelper;

        public CanvasListItemHandler(
            ISerializationHelper serializer,
            ITextProcessor textProcessor,
            IContentFragmentHelper contentFragmentHelper)
        {
            _serializer = serializer;
            _textProcessor = textProcessor;
            _contentFragmentHelper = contentFragmentHelper;
        }

        string IContentHandler.ContentType => CanvasKeys.LIST;

        public bool CanHandle(string className)
        {
            return className == typeof(CanvasListItem).Name
                || className == typeof(CanvasListBlock).Name
                || className == CanvasKeys.LIST;
        }

        public async Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            var htmlContent = new HtmlContentBuilder();
            var canvasItem = item.Content;

            if (canvasItem == null)
                return htmlContent;

            try
            {
                string? content = canvasItem.ToString();
                JToken token = JToken.Parse(content.SafeString());

                if (token.Type == JTokenType.Array)
                {
                    // Bare _listItem[] — wrap in a synthetic _list and use the
                    // same recursive renderer as the object path.
                    htmlContent.AppendHtml(HandleArrayToken(token));
                }
                else if (token.Type == JTokenType.Object)
                {
                    // Normal _list object — deserialize and render recursively.
                    var listBlock = await _serializer.DeserializeAsync<CanvasListBlock>(item);
                    if (listBlock != null)
                    {
                        htmlContent.AppendHtml(HandleListBlock(listBlock));
                    }
                }
            }
            catch (Exception ex)
            {
                htmlContent.AppendHtml(
                    $"<!-- Error processing CanvasList Block Item: {ex.Message} -->");
            }

            return htmlContent;
        }

        /// <summary>
        /// Renders a <see cref="CanvasListBlock"/> via the recursive extension,
        /// which handles nested lists inside list items.
        /// </summary>
        private static IHtmlContent HandleListBlock(CanvasListBlock listBlock)
        {
            string html = listBlock.RenderHtmlRecursive("shade-black");
            return new HtmlString(html);
        }

        /// <summary>
        /// Handles a bare array of list items by wrapping it in a synthetic
        /// <see cref="CanvasListBlock"/> and going through the same recursive
        /// renderer. Previously this path used CanvasListItemRenderer which
        /// did not handle nested lists.
        /// </summary>
        private static IHtmlContent HandleArrayToken(JToken token)
        {
            var wrapped = new JObject
            {
                ["id"] = "synthetic-list",
                ["type"] = CanvasKeys.LIST,
                ["properties"] = new JObject { ["listType"] = "unordered" },
                ["value"] = token
            };

            var listBlock = wrapped.ToObject<CanvasListBlock>();
            if (listBlock == null)
                return HtmlString.Empty;

            string html = listBlock.RenderHtmlRecursive("shade-black");
            return new HtmlString(html);
        }
    }
}