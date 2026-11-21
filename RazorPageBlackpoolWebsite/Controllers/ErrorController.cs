using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RazorPageBlackpoolWebsite.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("")]
        [Route("{statusCode:int?}")]
        public IActionResult Index(int? statusCode = null)
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (feature?.Error is not null)
            {
                _logger.LogError(feature.Error,
                    "Unhandled exception on {Path}",
                    feature.Path);
            }
            else if (statusCode is not null && statusCode >= 500)
            {
                _logger.LogWarning("Error page served for status {StatusCode} on {Path}",
                    statusCode, HttpContext.Request.Path);
            }

            Response.StatusCode = statusCode ?? 500;
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}