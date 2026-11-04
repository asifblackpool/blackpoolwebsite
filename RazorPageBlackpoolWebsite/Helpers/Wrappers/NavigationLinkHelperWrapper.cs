using Content.Modelling.Models.Canvas.Paragraphs;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;

namespace RazorPageBlackpoolWebsite.Helpers.Wrappers
{
    public class NavigationLinkHelperWrapper : INavigationLinkHelper
    {
        private readonly ISiteConfig _siteConfig;

        public NavigationLinkHelperWrapper(ISiteConfig siteConfig)
        {
            _siteConfig = siteConfig ?? throw new ArgumentNullException(nameof(siteConfig));
        }

        public Task<string> GetLinkUrlAsync(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<FragmentParagraph> GetLinkUrlAsync(FragmentParagraph fragment)
        {
            if (fragment == null)
            {
                throw new ArgumentNullException(nameof(fragment));
            }

            try
            {
                // Option 1: If GetLinkUrl is CPU-bound (synchronous)
                return await Task.Run(() =>
                {
                    return NavigationLinkHelper.GetLinkUrl(fragment, _siteConfig.ViewFolder);
                });

                /*
                // Option 2: If you can make NavigationLinkHelper async (preferred)
                return await NavigationLinkHelper.GetLinkUrlAsync(fragment, cancellationToken)
                       .ConfigureAwait(false);
                */
            }
            catch (OperationCanceledException)
            {
                throw; // Re-throw to let caller handle cancellation
            }
            catch (Exception)
            {
                return fragment; // Return original as fallback
            }
        }
    }
}