using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Zengenti.Contensis.Delivery;
using Microsoft.AspNetCore.Rewrite;
using Content.Modelling.Extensions;
using RazorPageBlackpoolWebsite.Core.Interfaces;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling;
using RazorPageBlackpoolWebsite.Core.Services.ContentProcessing.Interfaces;
using RazorPageBlackpoolWebsite.Core.Services.Processors;
using RazorPageBlackpoolWebsite.Helpers;
using RazorPageBlackpoolWebsite.Helpers.Html;
using RazorPageBlackpoolWebsite.Helpers.Interfaces;
using RazorPageBlackpoolWebsite.Helpers.Renderers;
using RazorPageBlackpoolWebsite.Helpers.Renderers.Components;
using RazorPageBlackpoolWebsite.Helpers.Serialisation;
using RazorPageBlackpoolWebsite.Helpers.Wrappers;
using RazorPageBlackpoolWebsite.Middleware;
using RazorPageBlackpoolWebsite.Services;
using RazorPageBlackpoolWebsite.Services.Breadcrumb;
using RazorPageBlackpoolWebsite.Services.Interfaces;
using RazorPageBlackpoolWebsite.Infrastructure.Repositories;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Handlers;
using RazorPageBlackpoolWebsite.Core.Services.ContentHandling.Interfaces;
using RazorPageBlackpoolWebsite.Constants.Interfaces;
using RazorPageBlackpoolWebsite.Configuration;

// Load environment variables FIRST.
// LoadOptions(false) = do NOT overwrite variables that are already set
// (launch profile on local dev, Contensis block env on deployment).
// This is what makes launch-profile section switching work locally,
// and what lets the Contensis block's env win over .env on deployment.
DotNetEnv.Env.Load(
    path: Directory.GetCurrentDirectory(),
    options: new LoadOptions(false)
);

var builder = WebApplication.CreateBuilder(args);

// Bind settings
builder.Services.Configure<BlackpoolSettings>(
    builder.Configuration.GetSection("BlackpoolSettings"));

// Register resolver
builder.Services.AddScoped<ApiUrlResolver>();

// Configure Forwarded Headers to trust the proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.AllowedHosts.Clear();
});

builder.Services.AddScoped<IContensisClientResolver, ContensisClientResolver>();

// Register the concrete ContensisClient so that any class expecting it gets the correct per‑request client
builder.Services.AddScoped<ContensisClient>(sp =>
{
    var resolver = sp.GetRequiredService<IContensisClientResolver>();
    return resolver.GetClient();
});

builder.Services.AddSingleton<ISiteConfig, SiteConfig>();

// Register generic data service (this depends on IContensisClient)
builder.Services.AddScoped(typeof(IDataService<>), typeof(ContensisDataService<>));
builder.Services.AddTransient<IContentRepository, ContensisContentRepository>();

// Register helpers
builder.Services.AddScoped<ISerializationHelper, SerializationHelper>();
builder.Services.AddScoped<ICanvasPanelHelper, CanvasPanelHelperWrapper>();
builder.Services.AddScoped<IPanelHelper, PanelHelperWrapper>();
builder.Services.AddScoped<IParagraphHelper, ParagraphHelperWrapper>();
builder.Services.AddScoped<INavigationLinkHelper, NavigationLinkHelperWrapper>();
builder.Services.AddScoped<IFormHelper, FormHelperWrapper>();
builder.Services.AddScoped<IContentFragmentHelper, ContentFragmentHelper>();
builder.Services.AddScoped<IImageHelper, ImageHelperWrapper>();
builder.Services.AddScoped<ITableHelper, TableHelperWrapper>();
builder.Services.AddScoped<IAccordionRenderer, AccordionRenderer>();
builder.Services.AddScoped<IBgCtaLinkRenderer, BgCtaLinkRenderer>();
builder.Services.AddScoped<IGovUkAccordionWithCtaButtonRenderer, GovUkAccordionWithCtaButtonRenderer>();
builder.Services.AddScoped<IGovUkAccordionWithImagesRenderer, GovUkAccordionWithImagesRenderer>();
builder.Services.AddScoped<IGovUkAccordionRenderer, GovUkAccordionRenderer>();
builder.Services.AddScoped<ViewComponentRenderer>();

// ── Privacy Notices accordion ──
builder.Services.AddPrivacyNoticesAccordion();
builder.Services.AddScoped<IPrivacyNoticesAccordionRenderer, PrivacyNoticesAccordionRenderer>();
builder.Services.AddScoped<IContentHandler, PrivacyNoticesAccordionHandler>();

builder.Services.AddScoped<IWebCastRenderer, WebCastRenderer>();

// Processors
builder.Services.AddScoped<ITextProcessor, HtmlTextProcessor>();

// Configure logging
builder.Services.AddLogging(configure =>
    configure.AddConsole().SetMinimumLevel(LogLevel.Information));

// Register IZengentiClient adapter and ContentViewModelService
builder.Services.AddScoped<IZengentiClient, ZengentiClientAdapter>();
builder.Services.AddScoped<ContentViewModelService>();

// ===== MVC Controllers =====
builder.Services.AddControllersWithViews();

// Required by PdfController to fetch files from Contensis storage
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("ClassicAssetProxy", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ===== Razor Pages =====
builder.Services
    .AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.RootDirectory = "/Pages";
        options.Conventions.Add(new GlobalHeaderPageApplicationModelConvention());
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<BreadcrumbService>();

// Automatic register all content handlers
builder.Services.AddContentHandlers();

// Add all content modelling services (one line!)
builder.Services.AddContentModelling(builder.Configuration, options =>
{
    options.DefaultCacheMinutes = 10;
    options.DebugTokenKey = "DebugToken";
    options.EnableDebugModeByDefault = false;
});

// Register the factory that maps Contensis content types to view models
builder.Services.AddScoped<ICmsViewModelFactory, CmsViewModelFactory>();

// ===== In-memory cache with BYTE-based size limit =====
//
// The ZengentiClientAdapter and CanvasHydrator use byte-based sizes
// (SetSize(serialisedPayloadLength)). That means the SizeLimit here is
// in BYTES, not entries.
//
// REDUCED from 64 MB to 16 MB — the container is 512 MB and the image
// was already at 88.6% (454 MB) before any request, leaving only ~58 MB
// of headroom. 64 MB of cache on top of that caused OOM on first request.
// 16 MB is enough for a few dozen hydrated pages; raise only if you see
// frequent cache misses AND the image is well under the memory limit.
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 16 * 1024 * 1024;   // 16 MB total
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("RequestTrace");
    logger.LogInformation("INCOMING {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
});

var siteConfig = app.Services.GetRequiredService<ISiteConfig>();

// ─────────────────────────────────────────────────────────────────────
// EXCEPTION HANDLING — MUST BE FIRST
// ─────────────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandlingPath = "/Error",
        AllowStatusCode404Response = true
    });
    app.UseHsts();
}
else
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
        await next();
    });
}

// ─────────────────────────────────────────────────────────────────────
// EARLY DIAGNOSTIC MIDDLEWARE
//
// Runs BEFORE UseContentModelling, UseStaticFiles, ClassicAssetProxy,
// UseRewriter — i.e. everything that throws before routing. If an
// exception happens in any of those, this catches it and logs the
// full stack trace before UseExceptionHandler swallows it.
//
// TEMPORARY — remove once the 500 root cause is found.
// ─────────────────────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("EarlyDiagnostic");

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "EARLY UNHANDLED {ExceptionType} on {Path}",
            ex.GetType().Name,
            context.Request.Path);
        throw;
    }
});

app.UseContentModelling();
app.UseForwardedHeaders();

app.UseStaticFiles();

// Proxy asset requests to the Classic backend so PDFs, CSVs, Office
// documents etc. are served correctly instead of being routed to the
// Block renderer.
app.UseMiddleware<ClassicAssetProxyMiddleware>();


// Redirect root to the site root
app.UseRewriter(new RewriteOptions().AddRedirect("^$", siteConfig.SitePath, app.Environment.IsDevelopment() ? 302 : 301));

app.UseRouting();

// ─────────────────────────────────────────────────────────────────────
// AFTER-ROUTING DIAGNOSTIC MIDDLEWARE
//
// Runs after UseRouting() so context.GetEndpoint() returns the matched
// controller/action. Catches exceptions from controllers, Razor pages,
// and BreadcrumbMiddleware with the handler name in the log.
//
// TEMPORARY — remove once the 500 root cause is found.
// ─────────────────────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DiagnosticMiddleware");

    var endpoint = context.GetEndpoint()?.DisplayName;

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "UNHANDLED {ExceptionType} on {Path} — handler: {Handler}",
            ex.GetType().Name,
            context.Request.Path,
            endpoint);
        throw;
    }
});

// Capture the original path so the error page can display it.
app.UseErrorContext();


// ─────────────────────────────────────────────────────────────────────
// ENDPOINT REGISTRATION — ORDER MATTERS
// ─────────────────────────────────────────────────────────────────────
app.MapControllers();

// 1. EXACT match for /xxxxx (or /xxxxx) – must come first
app.MapControllerRoute(
    name: $"{siteConfig.SiteController}_root_exact",
    pattern: siteConfig.SitePath,
    defaults: new { controller = "Root", action = "Index", slug = "" }
);

app.MapControllerRoute(
    name: $"{siteConfig.SiteController}_section",
    pattern: siteConfig.SitePath + "/{section:regex(^(?!pdf$|error$).+)}/{**slug}",
    defaults: new { controller = "Section", action = "Index" }
);

app.MapRazorPages();

// Breadcrumb runs after routing so it can see the matched endpoint,
// but before status-code pages so it can log the original request.
app.UseMiddleware<BreadcrumbMiddleware>();

// Status-code re-execution LAST, so it catches 404s from everything above.
// Requires an endpoint at /Error — provided by ErrorController.
app.UseStatusCodePagesWithReExecute("/Error");

// ===== WARM UP CONTENSIS CLIENT =====
//
// Warm up the HTTP/TLS stack ONLY. Do not fetch a full entry — that can
// itself OOM at boot and leave the process in a degraded state before
// a single request has been served. A minimal node fetch with narrow
// fields exercises the same code path with negligible memory.
using (var warmupScope = app.Services.CreateScope())
{
    var logger = warmupScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var client = warmupScope.ServiceProvider.GetRequiredService<ContensisClient>();
        await client.Nodes.GetByPathAsync(
           siteConfig.SitePath.TrimStart('/'),
           entryFields: new[] { "title" },
           entryLinkDepth: 0);
        logger.LogInformation("Contensis client warmed up successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Contensis client warm-up failed – first request may be slow.");
    }
}

await app.RunAsync();

#region ContensisClientFactory
public static class ContensisClientFactory
{
    private static readonly object _lock = new object();
    private static bool _envLoaded = false;

    private static void EnsureEnvironmentLoaded()
    {
        if (!_envLoaded)
        {
            lock (_lock)
            {
                if (!_envLoaded)
                {
                    Env.TraversePath().Load();
                    _envLoaded = true;
                }
            }
        }
    }

    public static ContensisClient CreatePreviewClient()
    {
        EnsureEnvironmentLoaded();
        return ContensisClient.Create(
            projectId: Env.GetString("PROJECT_API_ID"),
            rootUrl: string.Format("https://api-{0}.cloud.contensis.com", Env.GetString("ALIAS")),
            clientId: Env.GetString("CONTENSIS_CLIENT_ID"),
            sharedSecret: Env.GetString("CONTENSIS_CLIENT_SECRET"),
            versionStatus: VersionStatus.Latest
        );
    }

    public static ContensisClient CreateLiveClient()
    {
        EnsureEnvironmentLoaded();
        return ContensisClient.Create(
            projectId: Env.GetString("PROJECT_API_ID"),
            rootUrl: string.Format("https://api-{0}.cloud.contensis.com", Env.GetString("ALIAS")),
            clientId: Env.GetString("CONTENSIS_CLIENT_ID"),
            sharedSecret: Env.GetString("CONTENSIS_CLIENT_SECRET"),
            versionStatus: VersionStatus.Published
        );
    }

    public static ContensisClient CreateClient(bool isPreview, string QueryStringversionStatus)
    {
        if (QueryStringversionStatus == "latest")
            return CreatePreviewClient();
        else if (QueryStringversionStatus == "published")
            return CreateLiveClient();
        else
            return isPreview ? CreatePreviewClient() : CreateLiveClient();
    }
}
#endregion