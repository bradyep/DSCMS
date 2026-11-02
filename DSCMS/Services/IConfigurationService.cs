using System;
using System.Collections.Generic;

namespace DSCMS.Services
{
    /// <summary>
    /// Interface for accessing application configuration settings
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Get a configuration value by key
        /// </summary>
        string? GetValue(string key);

        /// <summary>
        /// Get a strongly typed configuration value
        /// </summary>
        T GetValue<T>(string key);

        /// <summary>
        /// Get a configuration section as a dictionary
        /// </summary>
        Dictionary<string, string> GetSection(string sectionName);

        /// <summary>
        /// Get email settings
        /// </summary>
        EmailSettings GetEmailSettings();

        /// <summary>
        /// Get database settings
        /// </summary>
        DatabaseSettings GetDatabaseSettings();

        /// <summary>
        /// Get settings for admin-uploaded static files
        /// </summary>
        StaticFileSettings GetStaticFileSettings();

        /// <summary>
        /// Get settings for site backup and restore
        /// </summary>
        BackupSettings GetBackupSettings();
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = "noreply@dscms.local";
        public string FromName { get; set; } = "DSCMS System";
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = "";
        public string DatabaseType { get; set; } = "SQLite";
        public int CommandTimeout { get; set; } = 30;
    }

    public class StaticFileSettings
    {
        /// <summary>
        /// Folder uploads are stored in relative paths that resolve against the content root.
        /// Should live in persistent storage (the data volume in Docker), not in wwwroot.
        /// </summary>
        public string StoragePath { get; set; } = "dscms-data/static";
        public long MaxUploadBytes { get; set; } = 200L * 1024 * 1024;
        public long MaxExtractedBytes { get; set; } = 1024L * 1024 * 1024;
        public int MaxArchiveEntries { get; set; } = 5000;
    }

    public class BackupSettings
    {
        /// <summary>
        /// Folder for automatic pre-restore snapshots. Must be on persistent storage and never publicly served,
        /// because backups contain password hashes.
        /// </summary>
        public string StoragePath { get; set; } = "dscms-data/backups";
        public long MaxRestoreBytes { get; set; } = 2L * 1024 * 1024 * 1024;
        public long MaxExtractedBytes { get; set; } = 10L * 1024 * 1024 * 1024;
        public int MaxArchiveEntries { get; set; } = 50000;
        public int SnapshotsToKeep { get; set; } = 3;
    }
}
