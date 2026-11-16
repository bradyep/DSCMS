using System.Security.Cryptography;

namespace DSCMS.Services
{
  /// <summary>
  /// File-system helpers shared by static uploads and backup/restore: hashed, size-capped writes
  /// and "zip slip" safe path resolution for archive entries.
  /// </summary>
  internal static class SafeFileIO
  {
    private const int CopyBufferSize = 81920;

    /// <summary>
    /// Copies <paramref name="source"/> to a new file, returning its size and lowercase hex SHA-256.
    /// Counts bytes actually written rather than trusting any declared length.
    /// </summary>
    public static async Task<(long Size, string Sha256)> WriteFileAsync(Stream source, string destination, long maxBytes, Func<Exception> onTooLarge, CancellationToken cancellationToken)
    {
      await using var output = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true);
      return await CopyAndHashAsync(source, output, maxBytes, onTooLarge, cancellationToken);
    }

    /// <summary>
    /// Copies one stream to another, returning the byte count and lowercase hex SHA-256 of what was copied.
    /// </summary>
    public static async Task<(long Size, string Sha256)> CopyAndHashAsync(Stream source, Stream destination, long maxBytes, Func<Exception> onTooLarge, CancellationToken cancellationToken)
    {
      using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
      var buffer = new byte[CopyBufferSize];
      long total = 0;

      int read;
      while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
      {
        total += read;
        if (total > maxBytes)
        {
          throw onTooLarge();
        }

        hash.AppendData(buffer, 0, read);
        await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
      }

      return (total, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    public static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken = default)
    {
      await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, useAsync: true);
      var hash = await SHA256.HashDataAsync(stream, cancellationToken);
      return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// True if a '/'-separated relative path has no empty, "." or ".." segments.
    /// </summary>
    public static bool IsCleanRelativePath(string relativePath)
    {
      return relativePath.Length > 0 && !relativePath.Split('/').Any(s => s.Length == 0 || s == "." || s == "..");
    }

    /// <summary>
    /// Resolves a relative path under <paramref name="root"/>, or returns null if it would land outside it.
    /// </summary>
    public static string? ResolveInside(string root, string relativePath)
    {
      var rootPrefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
      var destination = Path.GetFullPath(Path.Combine(root, relativePath));
      return destination.StartsWith(rootPrefix, StringComparison.Ordinal) ? destination : null;
    }
  }
}
