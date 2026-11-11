using Content.Modelling.Constants;
using Content.Modelling.Models.Components;
using Content.Modelling.Models.GenericTypes;
using Content.Modelling.Models.Templates;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Newtonsoft.Json.Linq;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.ContextView;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;

namespace RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers
{
    /// <summary>
    /// Handles one or more consecutive "tile" canvas components.
    /// The CanvasHelper batches consecutive tiles into:
    ///   { "tile": [ {..}, {..}, ... ] }
    /// This handler renders them together via the TileNavigation view component.
    /// </summary>
    public class NavigationTileHandler : IContentHandler
    {
        private readonly IViewComponentHelper _viewComponentHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NavigationTileHandler(
            IViewComponentHelper viewComponentHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _viewComponentHelper = viewComponentHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public string ContentType => throw new NotImplementedException();

        public bool CanHandle(string className)
        {
            return className == ComponentKeys.NAVIGATION_TILE
                || className == typeof(NavigationTile).Name;
        }

        public async Task<IHtmlContent> HandleAsync(SerialisedItem item)
        {
            var htmlContent = new HtmlContentBuilder();

            try
            {
                if (string.IsNullOrEmpty(item.Content))
                    return htmlContent;

                var json = JObject.Parse(item.Content);
                var tilesArray = json[ComponentKeys.NAVIGATION_TILE] as JArray;

                if (tilesArray == null || tilesArray.Count == 0)
                    return htmlContent;

                var tiles = tilesArray.ToObject<List<NavigationTile>>();
                if (tiles == null || tiles.Count == 0)
                    return htmlContent;

                ViewContextHelper.EnsureViewContext(_viewComponentHelper, _httpContextAccessor);

                var result = await _viewComponentHelper.InvokeAsync("TileNavigation", new
                {
                    list = tiles,
                    layout = TileLayout.FourTilesInARow
                });

                htmlContent.AppendHtml(result);
            }
            catch (Exception ex)
            {
                htmlContent.AppendHtml(
                    $"<!-- Error processing the NavigationTile Handler: {ex.Message} -->");
            }

            return htmlContent;
        }
    }
}