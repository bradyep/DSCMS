using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;
using Microsoft.AspNetCore.Identity;

namespace DSCMS.Controllers
{
  /// <summary>
  /// Controller for diagnostic and maintenance utilities
  /// </summary>
  public class DiagnosticsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DiagnosticsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    /// <summary>
    /// Reset password for the current user to enable admin access
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ResetAdminPassword()
    {
      var users = await _context.Users.ToListAsync();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS Admin Password Reset Utility ===\n");
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database.", "text/plain");
      }
      
      var adminUser = users.FirstOrDefault();
      report.AppendLine($"Resetting password for user: {adminUser.DisplayName} ({adminUser.Email})");
      
      try
      {
        // Remove the existing password
        await _userManager.RemovePasswordAsync(adminUser);
        
        // Set new password to "Passw0rd"
        var result = await _userManager.AddPasswordAsync(adminUser, "Passw0rd");
        
        if (result.Succeeded)
        {
          report.AppendLine("? Password successfully reset to: Passw0rd");
          report.AppendLine($"? You can now log in to /Account/Login with:");
          report.AppendLine($"  Email: {adminUser.Email}");
          report.AppendLine($"  Password: Passw0rd");
          report.AppendLine($"? After logging in, navigate to /Admin to access the admin portal");
        }
        else
        {
          report.AppendLine("? Password reset failed:");
          foreach (var error in result.Errors)
          {
            report.AppendLine($"  - {error.Description}");
          }
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Alternative password reset with custom password
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ResetAdminPasswordCustom(string newPassword = "AdminPass123!")
    {
      var users = await _context.Users.ToListAsync();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS Custom Admin Password Reset ===\n");
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database.", "text/plain");
      }
      
      // Validate password complexity
      if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
      {
        return Content("ERROR: Password must be at least 6 characters long. Use: /Diagnostics/ResetAdminPasswordCustom?newPassword=YourPassword", "text/plain");
      }
      
      var adminUser = users.FirstOrDefault();
      report.AppendLine($"Resetting password for user: {adminUser.DisplayName} ({adminUser.Email})");
      report.AppendLine($"New password: {newPassword}");
      
      try
      {
        // Remove the existing password
        await _userManager.RemovePasswordAsync(adminUser);
        
        // Set new password
        var result = await _userManager.AddPasswordAsync(adminUser, newPassword);
        
        if (result.Succeeded)
        {
          report.AppendLine("? Password successfully reset!");
          report.AppendLine($"? You can now log in to /Account/Login with:");
          report.AppendLine($"  Email: {adminUser.Email}");
          report.AppendLine($"  Password: {newPassword}");
          report.AppendLine($"? After logging in, navigate to /Admin to access the admin portal");
        }
        else
        {
          report.AppendLine("? Password reset failed:");
          foreach (var error in result.Errors)
          {
            report.AppendLine($"  - {error.Description}");
          }
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Check and fix null values in Layouts table using Entity Framework
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixLayoutNullsEF()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Layout NULL Values Fix Utility (EF Version) ===\n");
      
      try
      {
        // Get all layouts and check for issues
        var layouts = await _context.Layouts.ToListAsync();
        report.AppendLine($"Found {layouts.Count} layout records:");
        
        var fixedCount = 0;
        
        foreach (var layout in layouts)
        {
          var needsFix = false;
          var issues = new List<string>();
          
          if (string.IsNullOrEmpty(layout.Name))
          {
            layout.Name = $"Layout {layout.LayoutId}";
            issues.Add("Name");
            needsFix = true;
          }
          
          if (string.IsNullOrEmpty(layout.FileLocation))
          {
            layout.FileLocation = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml";
            issues.Add("FileLocation");
            needsFix = true;
          }
          
          // FileContents can be null - that's acceptable for layouts that reference external files
          
          if (needsFix)
          {
            fixedCount++;
            report.AppendLine($"  Layout {layout.LayoutId}: Fixed {string.Join(", ", issues)}");
          }
          else
          {
            var hasInlineContent = layout.FileContents != null ? "with inline content" : "referencing external file";
            report.AppendLine($"  Layout {layout.LayoutId}: OK ({hasInlineContent})");
          }
        }
        
        if (fixedCount > 0)
        {
          await _context.SaveChangesAsync();
          report.AppendLine($"\n? Fixed {fixedCount} layout records!");
          report.AppendLine("? All required fields have been populated!");
          report.AppendLine("? You can now access /Admin without errors");
        }
        else
        {
          report.AppendLine("\n? No required fields missing in Layouts table");
        }
        
        // If no layouts exist, create a sample one
        if (layouts.Count == 0)
        {
          report.AppendLine("\nNo layouts found. Creating sample layout...");
          
          var sampleLayout = new Layout
          {
            Name = "Bootstrap Blog Layout",
            FileLocation = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml",
            FileContents = null // This layout references an external file, so no inline content
          };
          
          _context.Layouts.Add(sampleLayout);
          await _context.SaveChangesAsync();
          
          report.AppendLine("? Sample layout created successfully!");
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Create sample layout if none exist
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CreateSampleLayout()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Create Sample Layout Utility ===\n");
      
      try
      {
        var layoutCount = await _context.Layouts.CountAsync();
        report.AppendLine($"Current layout count: {layoutCount}");
        
        if (layoutCount == 0)
        {
          report.AppendLine("Creating sample layout...");
          
          var sampleLayout = new Layout
          {
            Name = "Bootstrap Blog Layout",
            FileLocation = "/Views/DSCMS/Layouts/_BootstrapBlog.cshtml",
            FileContents = null // This layout references an external file, so no inline content needed
          };
          
          _context.Layouts.Add(sampleLayout);
          await _context.SaveChangesAsync();
          
          report.AppendLine("? Sample layout created successfully!");
          report.AppendLine("? Layout references external file (no inline content)");
        }
        else
        {
          report.AppendLine("Layouts already exist, no action needed.");
        }
        
        report.AppendLine("\n? You can now access /Admin");
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Check and fix null values in Templates table using Entity Framework
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixTemplateNullsEF()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Template NULL Values Fix Utility (EF Version) ===\n");
      
      try
      {
        // Get all templates and check for issues
        var templates = await _context.Templates.Include(t => t.Layout).ToListAsync();
        report.AppendLine($"Found {templates.Count} template records:");
        
        var fixedCount = 0;
        
        foreach (var template in templates)
        {
          var needsFix = false;
          var issues = new List<string>();
          
          if (string.IsNullOrEmpty(template.Name))
          {
            template.Name = $"Template {template.TemplateId}";
            issues.Add("Name");
            needsFix = true;
          }
          
          if (string.IsNullOrEmpty(template.FileLocation))
          {
            var templateType = template.IsForContentType > 0 ? "ContentTypes" : "Contents";
            template.FileLocation = $"/Views/DSCMS/Templates/{templateType}/Template{template.TemplateId}.cshtml";
            issues.Add("FileLocation");
            needsFix = true;
          }
          
          // FileContents can be null - that's acceptable for templates that reference external files
          
          if (needsFix)
          {
            fixedCount++;
            report.AppendLine($"  Template {template.TemplateId}: Fixed {string.Join(", ", issues)}");
          }
          else
          {
            var hasInlineContent = template.FileContents != null ? "with inline content" : "referencing external file";
            var templateType = template.IsForContentType > 0 ? "ContentType" : "Content";
            report.AppendLine($"  Template {template.TemplateId} ({templateType}): OK ({hasInlineContent})");
          }
        }
        
        if (fixedCount > 0)
        {
          await _context.SaveChangesAsync();
          report.AppendLine($"\n? Fixed {fixedCount} template records!");
          report.AppendLine("? All required fields have been populated!");
          report.AppendLine("? You can now access /Admin/Templates without errors");
        }
        else
        {
          report.AppendLine("\n? No required fields missing in Templates table");
        }
        
        // If no templates exist, create sample ones
        if (templates.Count == 0)
        {
          report.AppendLine("\nNo templates found. Creating sample templates...");
          
          // Create a sample content template
          var sampleContentTemplate = new Template
          {
            Name = "Bootstrap Blog Post Template",
            FileLocation = "/Views/DSCMS/Templates/Contents/BootstrapBlogPost.cshtml",
            FileContents = null, // References external file
            IsForContentType = 0, // For Content
            LayoutId = (await _context.Layouts.FirstOrDefaultAsync())?.LayoutId
          };
          
          // Create a sample content type template  
          var sampleContentTypeTemplate = new Template
          {
            Name = "Bootstrap Blog Listing Template",
            FileLocation = "/Views/DSCMS/Templates/ContentTypes/BootstrapBlogContentType.cshtml", 
            FileContents = null, // References external file
            IsForContentType = 1, // For ContentType
            LayoutId = (await _context.Layouts.FirstOrDefaultAsync())?.LayoutId
          };
          
          _context.Templates.AddRange(sampleContentTemplate, sampleContentTypeTemplate);
          await _context.SaveChangesAsync();
          
          report.AppendLine("? Sample templates created successfully!");
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    private string GetDefaultLayoutContent()
    {
      return "<!DOCTYPE html>\n<html>\n<head>\n" +
             "  <meta charset=\"utf-8\" />\n" +
             "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\n" +
             "  <title>@ViewData[\"Title\"]</title>\n" +
             "  <link rel=\"stylesheet\" href=\"~/lib/bootstrap/dist/css/bootstrap.css\" />\n" +
             "</head>\n<body>\n" +
             "  <div class=\"container\">\n" +
             "    @RenderBody()\n" +
             "  </div>\n" +
             "</body>\n</html>";
    }
  }
}