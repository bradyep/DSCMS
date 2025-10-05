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
}