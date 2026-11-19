using DSCMS.Models;
using DSCMS.Repositories.Interfaces;

namespace DSCMS.Services
{
  /// <summary>
  /// Caches which URL paths belong to static uploads, built from the StaticUploadFiles rows.
  /// Call <see cref="Invalidate"/> whenever uploads change (create, replace, delete, restore).
  /// </summary>
  public interface IStaticUploadRegistry
  {
    Task<StaticUploadIndex> GetIndexAsync(CancellationToken cancellationToken = default);

    void Invalidate();
  }

  public class StaticUploadRegistry : IStaticUploadRegistry
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaticUploadRegistry> _logger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private volatile StaticUploadIndex? _index;
    private int _version;

    public StaticUploadRegistry(IServiceScopeFactory scopeFactory, ILogger<StaticUploadRegistry> logger)
    {
      _scopeFactory = scopeFactory;
      _logger = logger;
    }

    public async Task<StaticUploadIndex> GetIndexAsync(CancellationToken cancellationToken = default)
    {
      // Fast path without the lock: once loaded, requests never wait here
      var index = _index;
      if (index != null)
      {
        return index;
      }

      await _loadLock.WaitAsync(cancellationToken);
      try
      {
        // Checked again because another request may have loaded the index while this one waited for the lock
        index = _index;
        if (index != null)
        {
          return index;
        }

        // If an Invalidate() lands while we're loading, what we read may already be stale: use it for this request, but don't cache it
        var version = Volatile.Read(ref _version);

        List<StaticUpload> uploads;
        try
        {
          using var scope = _scopeFactory.CreateScope();
          uploads = await scope.ServiceProvider.GetRequiredService<IStaticUploadRepository>().GetAllWithFilesAsync();
        }
        catch (Exception ex)
        {
          // Not cached, so the next request tries again
          _logger.LogError(ex, "Could not load the static upload list; uploads won't be served until it loads");
          return StaticUploadIndex.Empty;
        }

        index = StaticUploadIndex.Build(uploads);
        if (version == Volatile.Read(ref _version))
        {
          _index = index;
        }

        _logger.LogDebug("Loaded static upload index: {FileCount} files", index.FileCount);
        return index;
      }
      finally
      {
        _loadLock.Release();
      }
    }

    public void Invalidate()
    {
      Interlocked.Increment(ref _version);
      _index = null;
    }
  }

  /// <summary>
  /// Lookup from URL path to file. Keys are lowercase paths without leading/trailing slashes ("games/idemon/js/game.js");
  /// values are the real paths, relative to the storage root, in their on-disk case. Matching ignores case so links work
  /// the same on Windows and Linux. That's never ambiguous: an upload can't hold two paths that differ only by case.
  /// </summary>
  public sealed class StaticUploadIndex
  {
    /// <summary>
    /// Same names and priority as ASP.NET Core's DefaultFilesOptions, which served uploads before.
    /// </summary>
    private static readonly string[] DefaultDocumentNames = { "default.htm", "default.html", "index.htm", "index.html" };

    public static readonly StaticUploadIndex Empty = new(new Dictionary<string, string>(), new Dictionary<string, string>());

    private readonly Dictionary<string, string> _files;
    private readonly Dictionary<string, string> _defaultDocuments;

    private StaticUploadIndex(Dictionary<string, string> files, Dictionary<string, string> defaultDocuments)
    {
      _files = files;
      _defaultDocuments = defaultDocuments;
    }

    public int FileCount => _files.Count;

    public bool TryGetFile(string urlPath, out string filePath)
    {
      return _files.TryGetValue(urlPath.ToLowerInvariant(), out filePath!);
    }

    /// <summary>
    /// The default document (such as index.html) for a folder inside an upload, if it has one.
    /// </summary>
    public bool TryGetDefaultDocument(string urlPath, out string filePath)
    {
      return _defaultDocuments.TryGetValue(urlPath.ToLowerInvariant(), out filePath!);
    }

    public static StaticUploadIndex Build(IEnumerable<StaticUpload> uploads)
    {
      var files = new Dictionary<string, string>(StringComparer.Ordinal);
      var defaultDocuments = new Dictionary<string, (string Path, int Priority)>(StringComparer.Ordinal);

      foreach (var upload in uploads)
      {
        foreach (var file in upload.Files)
        {
          var path = $"{upload.FolderPath}/{file.RelativePath}";
          files.TryAdd(path.ToLowerInvariant(), path);

          var slash = path.LastIndexOf('/');
          var priority = Array.FindIndex(DefaultDocumentNames, n => n.Equals(path[(slash + 1)..], StringComparison.OrdinalIgnoreCase));
          if (priority < 0)
          {
            continue;
          }

          var folderKey = path[..slash].ToLowerInvariant();
          if (!defaultDocuments.TryGetValue(folderKey, out var existing) || priority < existing.Priority)
          {
            defaultDocuments[folderKey] = (path, priority);
          }
        }
      }

      return new StaticUploadIndex(files, defaultDocuments.ToDictionary(d => d.Key, d => d.Value.Path, StringComparer.Ordinal));
    }
  }
}
