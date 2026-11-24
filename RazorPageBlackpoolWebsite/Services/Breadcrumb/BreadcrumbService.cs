using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Core.Models;
using System.Globalization;
using Zengenti;

namespace RazorPageBlackpoolWebsite.Services.Breadcrumb
{
    /// <summary>
    /// Builds breadcrumb navigation items from the request path.
    ///
    /// NOTE: This service is registered as Scoped in Program.cs.
    /// The _items / _autoGenerate fields are per-request state.
    /// Do NOT change this to Singleton without moving that state into
    /// HttpContext.Items or similar, otherwise breadcrumbs will leak
    /// between requests.
    /// </summary>
    public class BreadcrumbService
    {
        private readonly List<BreadcrumbItem> _items = new();
        private bool _autoGenerate = true;
        private readonly ISiteConfig _siteConfig;

        public BreadcrumbService(ISiteConfig siteConfig)
        {
            _siteConfig = siteConfig ?? throw new ArgumentNullException(nameof(siteConfig));
        }

        #region manual items (currently unused — kept for callers)

        public void AddItem(string title, string? url = null)
        {
            _items.Add(new BreadcrumbItem { Title = title, Url = url });
            _autoGenerate = false;
        }

        public void Reset()
        {
            _items.Clear();
            _autoGenerate = true;
        }

        public void EnableAutoGeneration() => _autoGenerate = true;
        public void DisableAutoGeneration() => _autoGenerate = false;

        #endregion

        public List<BreadcrumbItem> GetBreadcrumbs(HttpContext context)
        {
            var finalItems = new List<BreadcrumbItem>
    {
        // 1. Home
        new BreadcrumbItem { Title = "Home", Url = "/" }
    };

            // ViewFolder is the Contensis Site View folder ("Site") — used only
            // to skip that segment from the request path. It is NOT a page and
            // must NOT appear in breadcrumb URLs.
            string? viewFolder = _siteConfig.ViewFolder?
                .TrimStart('/')
                .TrimEnd('/');

            var path = context.Request.Path.Value ?? "";
            var segments = path
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            // Skip the view-folder segment if present at the start of the path.
            if (segments.Count > 0 &&
                !string.IsNullOrEmpty(viewFolder) &&
                segments[0].Equals(viewFolder, StringComparison.OrdinalIgnoreCase))
            {
                segments.RemoveAt(0);
            }

            // The first remaining segment is the real site root (e.g. "your-council").
            // That is what breadcrumb URLs should be rooted at.
            string? siteRoot = segments.Count > 0 ? segments[0] : null;

            if (_autoGenerate)
            {
                string accumulatedPath = "";

                foreach (var segment in segments)
                {
                    accumulatedPath += $"/{segment}";

                    var title = CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(segment.Replace("-", " ").ToLowerInvariant());

                    finalItems.Add(new BreadcrumbItem
                    {
                        Title = title,
                        Url = accumulatedPath.ToLowerInvariant()
                    });
                }
            }
            else if (_items.Count > 0)
            {
                // Merge manual items, skipping duplicates of what's already present.
                var existingTitles = finalItems
                    .Select(i => i.Title?.ToLowerInvariant() ?? "")
                    .ToHashSet();

                foreach (var item in _items)
                {
                    var key = item.Title?.ToLowerInvariant() ?? "";
                    if (!string.IsNullOrEmpty(key) && !existingTitles.Contains(key))
                    {
                        finalItems.Add(item);
                        existingTitles.Add(key);
                    }
                }
            }

            // Mark the last item as the current page — remove its link.
            if (finalItems.Count > 0)
            {
                finalItems[^1].Url = null;
            }

            // Sanity: drop the Site View folder crumb if it somehow slipped in.
            return finalItems
                .Where(i => !string.Equals(i.Title, _siteConfig.ViewFolder, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

    }

}