using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSCMS.Services
{
    /// <summary>
    /// Service for accessing application configuration with logging and type safety
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string? GetValue(string key)
        {
            var value = _configuration[key];
            _logger.LogDebug("Configuration value requested: {Key} = {Value}", key, value != null ? "***" : "null");
            return value;
        }

        public T GetValue<T>(string key)
        {
            try
            {
                var value = _configuration.GetValue<T>(key);
                _logger.LogDebug("Typed configuration value requested: {Key} (type: {Type})", key, typeof(T).Name);
                return value ?? default(T)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting typed configuration value for key: {Key}", key);
                return default(T)!;
            }
        }

        public Dictionary<string, string> GetSection(string sectionName)
        {
            try
            {
                var section = _configuration.GetSection(sectionName);
                var result = section.AsEnumerable()
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!);
                
                _logger.LogDebug("Configuration section requested: {SectionName} (found {Count} keys)", sectionName, result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration section: {SectionName}", sectionName);
                return new Dictionary<string, string>();
            }
        }

        public EmailSettings GetEmailSettings()
        {
            try
            {
                var emailSettings = new EmailSettings();
                _configuration.GetSection("EmailSettings").Bind(emailSettings);
                
                _logger.LogDebug("Email settings loaded: SmtpServer={SmtpServer}, Port={Port}", 
                    emailSettings.SmtpServer, emailSettings.SmtpPort);
                
                return emailSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email settings");
                return new EmailSettings
                {
                    SmtpServer = "localhost",
                    SmtpPort = 587,
                    EnableSsl = true,
                    FromEmail = "noreply@dscms.local",
                    FromName = "DSCMS System"
                };
            }
        }

        public DatabaseSettings GetDatabaseSettings()
        {
            try
            {
                var dbSettings = new DatabaseSettings
                {
                    ConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dscms.db",
                    DatabaseType = _configuration.GetValue<string>("DatabaseSettings:DatabaseType") ?? "SQLite",
                    CommandTimeout = _configuration.GetValue<int>("DatabaseSettings:CommandTimeout", 30)
                };
                
                _logger.LogDebug("Database settings loaded: Type={DatabaseType}, Timeout={CommandTimeout}", 
                    dbSettings.DatabaseType, dbSettings.CommandTimeout);
                
                return dbSettings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading database settings");
                return new DatabaseSettings
                {
                    ConnectionString = "Data Source=dscms.db",
                    DatabaseType = "SQLite",
                    CommandTimeout = 30
                };
            }
        }
    }
}