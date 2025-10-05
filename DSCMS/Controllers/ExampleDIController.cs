using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DSCMS.Data;
using DSCMS.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace DSCMS.Controllers
{
    /// <summary>
    /// Example controller demonstrating dependency injection usage
    /// </summary>
    [Authorize]
    public class ExampleDIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExampleDIController> _logger;
        private readonly IConfigurationService _configurationService;
        private readonly IEmailSender _emailSender;

        public ExampleDIController(
            ApplicationDbContext context,
            ILogger<ExampleDIController> logger,
            IConfigurationService configurationService,
            IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _configurationService = configurationService;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Example action showing all DI components working together
        /// </summary>
        public async Task<IActionResult> Demo()
        {
            _logger.LogInformation("Demo action called - demonstrating DI components");

            // Use the DbContext
            var contentCount = _context.Contents.Count();
            _logger.LogDebug("Found {ContentCount} contents in database", contentCount);

            // Use configuration service
            var emailSettings = _configurationService.GetEmailSettings();
            _logger.LogDebug("Email configured with server: {SmtpServer}", emailSettings.SmtpServer);

            var dbSettings = _configurationService.GetDatabaseSettings();
            _logger.LogDebug("Database type: {DatabaseType}", dbSettings.DatabaseType);

            // Use email sender
            await _emailSender.SendEmailAsync("test@example.com", "DI Demo", "This demonstrates dependency injection working!");

            // Create a simple view model to display the information
            var model = new
            {
                ContentCount = contentCount,
                EmailServer = emailSettings.SmtpServer,
                DatabaseType = dbSettings.DatabaseType,
                Message = "All dependency injection components are working correctly!"
            };

            ViewData["Title"] = "Dependency Injection Demo";
            return View(model);
        }
    }
}