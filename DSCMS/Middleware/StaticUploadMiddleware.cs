using DSCMS.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DSCMS.Middleware;

/// <summary>
/// Serves admin-uploaded static files at their own folder path (/games/shapeshifter/...), and also under the
/// legacy /static prefix (/static/games/shapeshifter/...).
/// Precedence: a CMS page at the same URL wins over an upload, and an upload wins over a wwwroot file
/// (this middleware runs before the wwwroot static files).
/// </summary>
public class StaticUploadMiddleware
{
  private const string OriginalPathKey = "DSCMS.StaticUpload.OriginalPath";
  private static readonly PathString AliasPath = StaticFileStorageService.RequestPath;

  private readonly RequestDelegate _next;
  private readonly IStaticUploadRegistry _registry;
  private readonly ILogger<StaticUploadMiddleware> _logger;
  private readonly StaticFileMiddleware _fileServer;

  public StaticUploadMiddleware(RequestDelegate next, IStaticUploadRegistry registry, IStaticFileStorageService storage,
    IWebHostEnvironment environment, ILoggerFactory loggerFactory)
  {
    _next = next;
    _registry = registry;
    _logger = loggerFactory.CreateLogger<StaticUploadMiddleware>();

    // The built-in middleware does the actual sending, so Range requests (audio/video seeking), ETags/304 and HEAD keep working.
    // Unknown types are allowed because uploaded games often ship assets (.data, .unityweb, ...) with no registered MIME type.
    var options = new StaticFileOptions
    {
      FileProvider = new PhysicalFileProvider(storage.RootPath),
      ContentTypeProvider = storage.ContentTypeProvider,
      ServeUnknownFileTypes = true,
      DefaultContentType = "application/octet-stream"
    };
    _fileServer = new StaticFileMiddleware(FileNotOnDisk, environment, Options.Create(options), loggerFactory);
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var request = context.Request;
    if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
    {
      await _next(context);
      return;
    }

    var originalPath = request.Path;
    var viaAlias = originalPath.StartsWithSegments(AliasPath, StringComparison.OrdinalIgnoreCase, out var aliasRemainder);
    var urlPath = (viaAlias ? aliasRemainder : originalPath).Value ?? string.Empty;
    var key = urlPath.Trim('/');
    if (key.Length == 0)
    {
      await _next(context);
      return;
    }

    var index = await _registry.GetIndexAsync(context.RequestAborted);
    var hasTrailingSlash = urlPath.EndsWith('/');
    string? filePath = null;
    var isFolder = false;

    if (!hasTrailingSlash && index.TryGetFile(key, out var file))
    {
      filePath = file;
    }
    else if (index.TryGetDefaultDocument(key, out var defaultDocument))
    {
      isFolder = true;
      filePath = defaultDocument;
    }

    if (filePath == null)
    {
      await _next(context);
      return;
    }

    // /static/... can't collide with a CMS page because "static" is a reserved content type name
    if (!viaAlias && await IsCmsPageAsync(context, key))
    {
      _logger.LogDebug("CMS page {Path} takes priority over static upload file {FilePath}", originalPath, filePath);
      await _next(context);
      return;
    }

    // Like DefaultFiles: a folder needs its trailing slash, or relative links inside its index page resolve one level too high
    if (isFolder && !hasTrailingSlash)
    {
      context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
      context.Response.Headers.Location = request.PathBase + originalPath + "/" + request.QueryString;
      return;
    }

    // Serve the file under its real on-disk path, which may differ in case from the URL (Linux file systems are case-sensitive)
    context.Items[OriginalPathKey] = originalPath;
    request.Path = "/" + filePath;
    try
    {
      await _fileServer.Invoke(context);
    }
    finally
    {
      request.Path = originalPath;
      context.Items.Remove(OriginalPathKey);
    }
  }

  private static async Task<bool> IsCmsPageAsync(HttpContext context, string key)
  {
    // CMS pages have at most two segments ({contentTypeName}/{contentUrl}), so deeper paths skip the database
    if (key.Count(c => c == '/') > 1)
    {
      return false;
    }

    var lookup = context.RequestServices.GetRequiredService<ICmsPageLookup>();
    return await lookup.IsCmsPageAsync(key);
  }

  /// <summary>
  /// The database lists the file but it's missing on disk: put the URL back and let the rest of the pipeline handle it.
  /// </summary>
  private Task FileNotOnDisk(HttpContext context)
  {
    if (context.Items.TryGetValue(OriginalPathKey, out var path) && path is PathString originalPath)
    {
      context.Request.Path = originalPath;
    }
    return _next(context);
  }
}
