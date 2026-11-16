using Microsoft.Data.Sqlite;

namespace DSCMS.Services
{
  /// <summary>
  /// Checks that the app can actually write to its SQLite database. When it can't, SQLite doesn't fail to open:
  /// it quietly opens the file read-only, and the problem only shows up later as "attempt to write a readonly
  /// database" on the first save. This usually means the database file (or its folder, where SQLite creates its
  /// journal) is owned by a different user than the one the app runs as.
  /// </summary>
  public static class DatabaseAccess
  {
    /// <summary>
    /// Returns a description of why the database can't be written, or null if it can (or doesn't exist yet).
    /// </summary>
    public static string? FindWriteProblem(string connectionString)
    {
      // Unpooled, so the check sees the file's current permissions rather than reusing a connection opened earlier
      var builder = new SqliteConnectionStringBuilder(connectionString) { Pooling = false };
      if (string.IsNullOrEmpty(builder.DataSource) || builder.DataSource == ":memory:" || builder.Mode == SqliteOpenMode.Memory)
      {
        return null;
      }

      var path = Path.GetFullPath(builder.DataSource);
      if (!File.Exists(path))
      {
        return null;
      }

      var fix = $"Make sure the file and its folder are owned by (or writable by) the user the app runs as ('{Environment.UserName}'), then restart the app.";

      using (var connection = new SqliteConnection(builder.ToString()))
      {
        connection.Open();
        if (SQLitePCL.raw.sqlite3_db_readonly(connection.Handle, "main") == 1)
        {
          return $"The app can't write to its database ({path}), so it opened it read-only and every save will fail. {fix}";
        }
      }

      // SQLite also needs to create its journal file next to the database
      var folder = Path.GetDirectoryName(path)!;
      try
      {
        using (File.Create(Path.Combine(folder, $".dscms-write-test-{Guid.NewGuid():N}"), 1, FileOptions.DeleteOnClose))
        {
        }
      }
      catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
      {
        return $"The app can't create files in its database folder ({folder}), so saves will fail. {fix}";
      }

      return null;
    }
  }
}
