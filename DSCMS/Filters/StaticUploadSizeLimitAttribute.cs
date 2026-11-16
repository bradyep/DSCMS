using DSCMS.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DSCMS.Filters;

/// <summary>
/// Raises the request body and multipart limits to StaticFileSettings.MaxUploadBytes for one action.
/// Works like [RequestSizeLimit] + [RequestFormLimits], but reads the limit from configuration.
/// Runs as a resource filter, so it only applies after [Authorize] has accepted the request.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StaticUploadSizeLimitAttribute : Attribute, IResourceFilter, IOrderedFilter
{
  // Run before the framework's own form-limit filters so ours wins
  public int Order => -1000;

  public void OnResourceExecuting(ResourceExecutingContext context)
  {
    var settings = context.HttpContext.RequestServices.GetRequiredService<IStaticFileStorageService>().Settings;

    // Allow some room for multipart boundaries and the other form fields
    var maxRequestBytes = settings.MaxUploadBytes + 1024 * 1024;

    var bodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (bodySizeFeature != null && !bodySizeFeature.IsReadOnly)
    {
      bodySizeFeature.MaxRequestBodySize = maxRequestBytes;
    }

    var formFeature = context.HttpContext.Features.Get<IFormFeature>();
    if (formFeature?.Form == null)
    {
      context.HttpContext.Features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, new FormOptions
      {
        MultipartBodyLengthLimit = maxRequestBytes
      }));
    }
  }

  public void OnResourceExecuted(ResourceExecutedContext context)
  {
  }
}
