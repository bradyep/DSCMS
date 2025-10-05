using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Configure logging first
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    // In development, add more detailed logging
    if (builder.Environment.IsDevelopment())
    {
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
    }

    // Add services to the container (equivalent to ConfigureServices)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Add application services with proper dependency injection
    builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    builder.Services.AddTransient<IEmailSender, EmailSender>();

    // Add MVC and Razor Pages
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Get logger for startup operations with error handling
    ILogger<Program>? logger = null;
    try
    {
        logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("DSCMS application starting up...");

        // Log some configuration info with error handling
        var config = app.Services.GetRequiredService<IConfigurationService>();
        var dbSettings = config.GetDatabaseSettings();
        logger.LogInformation("Database configured: {DatabaseType}", dbSettings.DatabaseType);
    }
    catch (Exception ex)
    {
        // If we can't get the logger, just continue - this prevents startup failures
        Console.WriteLine($"Warning: Could not initialize logging: {ex.Message}");
    }

    // Configure the HTTP request pipeline (equivalent to Configure method)
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        logger?.LogInformation("Running in Development mode");
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        logger?.LogInformation("Running in Production mode");
    }

    // Comment out HTTPS redirection for localhost development
    // app.UseHttpsRedirection();

    // Configure static files with custom MIME types safely
    var provider = new FileExtensionContentTypeProvider();
    var customMappings = new Dictionary<string, string>(provider.Mappings);
    customMappings[".odt"] = "application/vnd.oasis.opendocument.text";
    var customProvider = new FileExtensionContentTypeProvider(customMappings);

    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = customProvider
    });

    logger?.LogDebug("Static files configured with custom MIME types");

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure routes
    app.MapControllerRoute(
        name: "Account",
        pattern: "Admin/Account/{action=Index}/{id?}",
        defaults: new { controller = "Account" });

    app.MapControllerRoute(
        name: "Layouts",
        pattern: "Admin/Layouts/{action=Index}/{id?}",
        defaults: new { controller = "Layouts" });

    app.MapControllerRoute(
        name: "Templates",
        pattern: "Admin/Templates/{action=Index}/{id?}",
        defaults: new { controller = "Templates" });

    app.MapControllerRoute(
        name: "Contents",
        pattern: "Admin/Contents/{action=Index}/{id?}",
        defaults: new { controller = "Contents" });

    app.MapControllerRoute(
        name: "Users",
        pattern: "Admin/Users/{action=Index}/{id?}",
        defaults: new { controller = "Users" });

    app.MapControllerRoute(
        name: "ContentTypes",
        pattern: "Admin/ContentTypes/{action=Index}/{id?}",
        defaults: new { controller = "ContentTypes" });

    app.MapControllerRoute(
        name: "ContentItems",
        pattern: "Admin/ContentItems/{action=Index}/{id?}",
        defaults: new { controller = "ContentItems" });

    app.MapControllerRoute(
        name: "ContentTypeItems",
        pattern: "Admin/ContentTypeItems/{action=Index}/{id?}",
        defaults: new { controller = "ContentTypeItems" });

    app.MapControllerRoute(
        name: "ExampleDI",
        pattern: "Admin/ExampleDI/{action=Demo}/{id?}",
        defaults: new { controller = "ExampleDI" });

    app.MapControllerRoute(
        name: "DefaultAdmin",
        pattern: "Admin",
        defaults: new { controller = "Layouts", action = "Index" });

    // Add explicit route for Diagnostics controller before the catch-all route
    app.MapControllerRoute(
        name: "Diagnostics",
        pattern: "Diagnostics/{action=Index}",
        defaults: new { controller = "Diagnostics" });

    app.MapControllerRoute(
        name: "cms",
        pattern: "{contentTypeName}/{contentUrl?}",
        defaults: new { controller = "DSCMS", action = "Content" });

    // Default route
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=DSCMS}/{action=Content}/{contentTypeName?}/{contentUrl?}");

    app.MapRazorPages();

    logger?.LogInformation("All routes configured successfully");
    logger?.LogInformation("DSCMS application startup complete");

    app.Run();
}
catch (Exception ex)
{
    // Log startup errors to console since logging might not be available
    Console.WriteLine($"Fatal error during application startup: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    
    // Try to log to file as well
    try
    {
        var errorLogPath = Path.Combine(Directory.GetCurrentDirectory(), "startup-error.log");
        File.WriteAllText(errorLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Fatal startup error: {ex}");
        Console.WriteLine($"Error details written to: {errorLogPath}");
    }
    catch
    {
        // If we can't write to file, just continue
    }
    
    throw; // Re-throw to ensure the application doesn't start in a broken state
}
