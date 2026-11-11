using Content.Modelling.Models.Canvas.Lists;
using Newtonsoft.Json;

namespace RazorPageBlackpoolWebsite.Helpers.Extensions
{
    /// <summary>
    /// Helper extension methods for rendering lists.
    /// </summary>
    public static class CanvasListExtensions
    {
        public static bool IsOrdered(this CanvasListBlock list)
        {
            return list?.Properties?.ListType?.ToLower() == "ordered";
        }

        public static string GetHtmlTag(this CanvasListBlock list)
        {
            return list.IsOrdered() ? "ol" : "ul";
        }

        /// <summary>
        /// Renders the list as HTML, recursively handling nested lists.
        /// </summary>
        public static string RenderHtmlRecursive(this CanvasListBlock list, string cssClass = "shade-black")
        {
            if (list?.Value == null || list.Value.Count == 0)
                return string.Empty;

            var html = new System.Text.StringBuilder();
            string tag = list.GetHtmlTag();

            html.Append($"<{tag} class=\"{cssClass}\">");

            foreach (var item in list.Value)
            {
                string content = ExtractTextContent(item.Content);
                html.Append($"<li>{content}</li>");
            }

            html.Append($"</{tag}>");
            return html.ToString();
        }

        /// <summary>
        /// Extracts HTML from an <see cref="IValue"/> (handles SimpleValue,
        /// ComplexValue, and nested ListFragment recursively).
        /// </summary>
        private static string ExtractTextContent(IValue? value)
        {
            if (value == null)
                return string.Empty;

            if (value is SimpleValue simple)
            {
                return simple.Text ?? string.Empty;
            }

            if (value is ComplexValue complex)
            {
                var fragments = new List<string>();
                foreach (var fragment in complex.Fragments)
                {
                    // ADD THIS LINE
                    System.Diagnostics.Debug.WriteLine($"[Extract] fragment: {fragment.GetType().Name}, Text={fragment.Text}");
                    if (fragment is TextFragment text)
                    {
                        fragments.Add(text.Text ?? string.Empty);
                    }
                    else if (fragment is HtmlFragment html)
                    {
                        fragments.Add(html.HtmlContent ?? string.Empty);
                    }
                    else if (fragment is LinkFragment link)
                    {
                        fragments.Add($"<a href=\"{link.Url}\">{link.Text}</a>");
                    }
                    else if (fragment is ImageFragment image)
                    {
                        fragments.Add($"<img src=\"{image.Source}\" alt=\"{image.AltText}\" />");
                    }
                    else if (fragment is ListFragment nestedList)
                    {
                        // ── NEW: recurse into nested lists ──
                        // ADD THIS LINE
                        System.Diagnostics.Debug.WriteLine($"[Extract] ListFragment found! Items={nestedList.Items?.Count ?? -1}");
                        var nested = new CanvasListBlock
                        {
                            Value = nestedList.Items,
                            Properties = new CanvasListProperties
                            {
                                ListType = nestedList.Properties?.ListType ?? "unordered"
                            }
                        };
                        fragments.Add(nested.RenderHtmlRecursive(cssClass: "shade-black"));
                    }
                    else
                    {
                        fragments.Add(fragment.Text ?? string.Empty);
                    }
                }
                return string.Join("", fragments);
            }

            return string.Empty;
        }

        public static CanvasListBlock? DeserializeListBlock(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ValueConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.DeserializeObject<CanvasListBlock>(json, settings);
        }
    }
}