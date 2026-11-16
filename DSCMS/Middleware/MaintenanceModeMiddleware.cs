using DSCMS.Services;

namespace DSCMS.Middleware;

/// <summary>
/// While a restore swaps the database and static files, answers everything except the backup API with 503.
/// While a backup is being written, rejects other admin API writes so the archive stays consistent.
/// </summary>
public class MaintenanceModeMiddleware
{
  private static readonly PathString BackupApiPath = "/api/backup";
  private static readonly PathString ApiPath = "/api";

  private readonly RequestDelegate _next;
  private readonly MaintenanceState _state;

  public MaintenanceModeMiddleware(RequestDelegate next, MaintenanceState state)
  {
    _next = next;
    _state = state;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var path = context.Request.Path;

    if (_state.IsBusy && !path.StartsWithSegments(BackupApiPath, StringComparison.OrdinalIgnoreCase))
    {
      if (_state.IsRestoring)
      {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "30";
        await context.Response.WriteAsync("The site is being restored from a backup. Please try again in a moment.");
        return;
      }

      if (path.StartsWithSegments(ApiPath, StringComparison.OrdinalIgnoreCase)
        && !HttpMethods.IsGet(context.Request.Method)
        && !HttpMethods.IsHead(context.Request.Method))
      {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { error = "A backup is in progress. Please try again in a moment." });
        return;
      }
    }

    await _next(context);
  }
}
