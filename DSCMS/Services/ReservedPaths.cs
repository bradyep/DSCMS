using Microsoft.AspNetCore.Mvc;

namespace DSCMS.Services
{
  /// <summary>
  /// First URL segments the app serves itself, which content type names and static upload folders can't use.
  /// </summary>
  public static class ReservedPaths
  {
    /// <summary>
    /// "static" is the backward-compatible upload alias, "admin"/"api"/"identity" are app routes,
    /// and css/js/lib hold the admin UI's own files in wwwroot (uploads take priority over wwwroot,
    /// so an upload there could break the admin).
    /// </summary>
    private static readonly HashSet<string> FixedNames = new(StringComparer.OrdinalIgnoreCase)
    {
      StaticFileStorageService.RequestPath.TrimStart('/'),
      "admin",
      "api",
      "identity",
      "css",
      "js",
      "lib"
    };

    /// <summary>
    /// The "default" route ({controller}/{action}/...) makes every controller name a dynamic first segment
    /// (/Home/..., /Account/..., /Diagnostics/...). Found by reflection so new controllers are covered automatically.
    /// </summary>
    private static readonly Lazy<HashSet<string>> UploadFolderNames = new(() =>
    {
      var names = new HashSet<string>(FixedNames, StringComparer.OrdinalIgnoreCase);
      var controllerNames = typeof(ReservedPaths).Assembly.GetTypes()
        .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(t => t.Name.EndsWith("Controller", StringComparison.Ordinal) ? t.Name[..^"Controller".Length] : t.Name);
      names.UnionWith(controllerNames);
      return names;
    });

    public static bool IsReservedContentTypeName(string? name)
    {
      return !string.IsNullOrWhiteSpace(name) && FixedNames.Contains(name.Trim());
    }

    /// <summary>
    /// Returns the folder's first segment if it is reserved, otherwise null.
    /// </summary>
    public static string? FindReservedUploadSegment(string folderPath)
    {
      var firstSegment = folderPath.Split('/')[0];
      return UploadFolderNames.Value.Contains(firstSegment) ? firstSegment : null;
    }
  }
}
