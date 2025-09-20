using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;

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

    /// <summary>
    /// Database diagnostics to check content and user relationships
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DatabaseStatus()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS Database Status Report ===\n");
      
      try
      {
        // Check Users
        var userCount = await _context.Users.CountAsync();
        report.AppendLine($"Users: {userCount}");
        
        var users = await _context.Users.Take(5).ToListAsync();
        foreach (var user in users)
        {
          report.AppendLine($"  User: {user.Id} - {user.DisplayName} ({user.Email})");
        }
        
        // Check Contents
        var contentCount = await _context.Contents.CountAsync();
        report.AppendLine($"\nContents: {contentCount}");
        
        var contents = await _context.Contents
          .Include(c => c.CreatedByUser)
          .Include(c => c.LastUpdatedByUser)
          .Take(5)
          .ToListAsync();
          
        foreach (var content in contents)
        {
          report.AppendLine($"\n  Content ID: {content.ContentId}");
          report.AppendLine($"    Title: {content.Title}");
          report.AppendLine($"    URL: {content.UrlToDisplay}");
          report.AppendLine($"    Body Length: {content.Body?.Length ?? 0}");
          report.AppendLine($"    Body Preview: {content.Body?.Substring(0, Math.Min(100, content.Body?.Length ?? 0)) ?? "NULL"}");
          report.AppendLine($"    CreatedBy: {content.CreatedBy} (User: {content.CreatedByUser?.DisplayName ?? "NULL"})");
          report.AppendLine($"    LastUpdatedBy: {content.LastUpdatedBy} (User: {content.LastUpdatedByUser?.DisplayName ?? "NULL"})");
          report.AppendLine($"    ContentTypeId: {content.ContentTypeId}");
          report.AppendLine($"    TemplateId: {content.TemplateId}");
        }
        
        // Check for orphaned foreign keys
        report.AppendLine("\n=== Foreign Key Issues ===");
        var contentsWithInvalidUsers = await _context.Contents
          .Where(c => c.CreatedBy != null && !_context.Users.Any(u => u.Id == c.CreatedBy))
          .CountAsync();
        report.AppendLine($"Contents with invalid CreatedBy references: {contentsWithInvalidUsers}");
        
        var contentsWithInvalidUpdaters = await _context.Contents
          .Where(c => c.LastUpdatedBy != null && !_context.Users.Any(u => u.Id == c.LastUpdatedBy))
          .CountAsync();
        report.AppendLine($"Contents with invalid LastUpdatedBy references: {contentsWithInvalidUpdaters}");
        
        // Check ContentTypes
        var contentTypeCount = await _context.ContentTypes.CountAsync();
        report.AppendLine($"\nContentTypes: {contentTypeCount}");
        
        // Check Templates
        var templateCount = await _context.Templates.CountAsync();
        report.AppendLine($"Templates: {templateCount}");
        
        // Test a specific content query
        var blogContentType = await _context.ContentTypes
          .Where(ct => ct.Name.ToLower() == "blog")
          .FirstOrDefaultAsync();
          
        if (blogContentType != null)
        {
          report.AppendLine($"\nBlog ContentType found: {blogContentType.ContentTypeId}");
          
          var blogContents = await _context.Contents
            .Where(c => c.ContentTypeId == blogContentType.ContentTypeId)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .ToListAsync();
            
          report.AppendLine($"Blog posts found: {blogContents.Count}");
          foreach (var blog in blogContents)
          {
            report.AppendLine($"  {blog.UrlToDisplay}: Body={blog.Body?.Length ?? 0} chars");
          }
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
    /// Fix orphaned content user references
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixContentUserReferences()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Fix Content User References ===\n");
      
      try
      {
        // Find contents with invalid user references
        var allContents = await _context.Contents.ToListAsync();
        var allUserIds = await _context.Users.Select(u => u.Id).ToListAsync();
        
        report.AppendLine($"Total contents: {allContents.Count}");
        report.AppendLine($"Total users: {allUserIds.Count}");
        
        var fixedCount = 0;
        
        foreach (var content in allContents)
        {
          var needsUpdate = false;
          
          // Check CreatedBy
          if (!string.IsNullOrEmpty(content.CreatedBy) && !allUserIds.Contains(content.CreatedBy))
          {
            report.AppendLine($"Content {content.ContentId} has invalid CreatedBy: {content.CreatedBy}");
            content.CreatedBy = null; // Set to null since the user doesn't exist
            needsUpdate = true;
          }
          
          // Check LastUpdatedBy  
          if (!string.IsNullOrEmpty(content.LastUpdatedBy) && !allUserIds.Contains(content.LastUpdatedBy))
          {
            report.AppendLine($"Content {content.ContentId} has invalid LastUpdatedBy: {content.LastUpdatedBy}");
            content.LastUpdatedBy = null; // Set to null since the user doesn't exist
            needsUpdate = true;
          }
          
          if (needsUpdate)
          {
            fixedCount++;
          }
        }
        
        if (fixedCount > 0)
        {
          await _context.SaveChangesAsync();
          report.AppendLine($"\n? Fixed {fixedCount} content records with invalid user references");
        }
        else
        {
          report.AppendLine("\n? No invalid user references found");
        }
        
        // If we have content but no users, create a default admin user
        if (allContents.Any() && !allUserIds.Any())
        {
          report.AppendLine("\nCreating default admin user for content...");
          
          var adminUser = new ApplicationUser
          {
            UserName = "admin@dscms.local",
            Email = "admin@dscms.local",
            DisplayName = "Admin User",
            EmailConfirmed = true
          };
          
          var result = await _userManager.CreateAsync(adminUser, "AdminPass123!");
          if (result.Succeeded)
          {
            report.AppendLine($"? Created admin user: {adminUser.Email}");
            
            // Optionally assign this user to all content
            foreach (var content in allContents.Where(c => string.IsNullOrEmpty(c.CreatedBy)))
            {
              content.CreatedBy = adminUser.Id;
              content.LastUpdatedBy = adminUser.Id;
            }
            
            await _context.SaveChangesAsync();
            report.AppendLine($"? Assigned admin user to {allContents.Count(c => string.IsNullOrEmpty(c.CreatedBy))} content items");
          }
          else
          {
            report.AppendLine("? Failed to create admin user:");
            foreach (var error in result.Errors)
            {
              report.AppendLine($"  - {error.Description}");
            }
          }
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
    /// Check ContentItems specifically to diagnose teaser text and other content item issues
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckContentItems()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== ContentItems Diagnostic Report ===\n");
      
      try
      {
        // Check ContentTypeItems first
        var contentTypeItems = await _context.ContentTypeItems
          .Include(cti => cti.ContentType)
          .ToListAsync();
        report.AppendLine($"Total ContentTypeItems: {contentTypeItems.Count}");
        
        foreach (var cti in contentTypeItems)
        {
          report.AppendLine($"  ContentTypeItem: {cti.Name} (ID: {cti.ContentTypeItemId}) for ContentType: {cti.ContentType?.Name}");
        }
        
        // Check ContentItems
        var contentItems = await _context.ContentItems
          .Include(ci => ci.ContentTypeItem)
          .Include(ci => ci.Content)
          .ToListAsync();
        report.AppendLine($"\nTotal ContentItems: {contentItems.Count}");
        
        var groupedByContent = contentItems.GroupBy(ci => ci.ContentId);
        foreach (var group in groupedByContent.Take(10))
        {
          var content = group.First().Content;
          report.AppendLine($"\n  Content: {content?.Title} ({content?.UrlToDisplay})");
          
          foreach (var ci in group)
          {
            var valuePreview = ci.Value?.Length > 50 ? ci.Value.Substring(0, 50) + "..." : ci.Value ?? "NULL";
            report.AppendLine($"    - {ci.ContentTypeItem?.Name}: \"{valuePreview}\"");
          }
        }
        
        // Check specific blog posts for TeaserText and Subject
        report.AppendLine("\n=== Blog Post ContentItems Check ===");
        var blogContentType = await _context.ContentTypes
          .Where(ct => ct.Name.ToLower() == "blog")
          .FirstOrDefaultAsync();
          
        if (blogContentType != null)
        {
          var blogPosts = await _context.Contents
            .Where(c => c.ContentTypeId == blogContentType.ContentTypeId)
            .Include(c => c.ContentItems)
            .ThenInclude(ci => ci.ContentTypeItem)
            .Take(5)
            .ToListAsync();
            
          foreach (var blog in blogPosts)
          {
            report.AppendLine($"\nBlog: {blog.Title} ({blog.UrlToDisplay})");
            report.AppendLine($"  ContentItems count: {blog.ContentItems.Count}");
            
            var teaserText = blog.ContentItems.FirstOrDefault(ci => ci.ContentTypeItem?.Name?.ToLower() == "teasertext");
            var subject = blog.ContentItems.FirstOrDefault(ci => ci.ContentTypeItem?.Name?.ToLower() == "subject");
            
            report.AppendLine($"  TeaserText: {teaserText?.Value ?? "NOT FOUND"}");
            report.AppendLine($"  Subject: {subject?.Value ?? "NOT FOUND"}");
            
            // List all content items for this blog post
            foreach (var ci in blog.ContentItems)
            {
              report.AppendLine($"    - {ci.ContentTypeItem?.Name}: \"{ci.Value}\"");
            }
          }
        }
        
        // Check for orphaned ContentItems
        report.AppendLine("\n=== Orphaned ContentItems ===");
        var orphanedItems = await _context.ContentItems
          .Where(ci => !_context.Contents.Any(c => c.ContentId == ci.ContentId))
          .CountAsync();
        report.AppendLine($"Orphaned ContentItems (no matching Content): {orphanedItems}");
        
        var orphanedTypeItems = await _context.ContentItems
          .Where(ci => !_context.ContentTypeItems.Any(cti => cti.ContentTypeItemId == ci.ContentTypeItemId))
          .CountAsync();
        report.AppendLine($"Orphaned ContentItems (no matching ContentTypeItem): {orphanedTypeItems}");
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Rebuild missing ContentItems for blog posts (Subject and TeaserText)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RebuildBlogContentItems()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Rebuild Blog ContentItems Utility ===\n");
      
      try
      {
        // First, ensure ContentTypeItems exist for blog
        var blogContentType = await _context.ContentTypes
          .Include(ct => ct.ContentTypeItems)
          .Where(ct => ct.Name.ToLower() == "blog")
          .FirstOrDefaultAsync();
          
        if (blogContentType == null)
        {
          report.AppendLine("ERROR: Blog content type not found!");
          return Content(report.ToString(), "text/plain");
        }
        
        report.AppendLine($"Found blog ContentType: {blogContentType.Name} (ID: {blogContentType.ContentTypeId})");
        
        // Check/Create ContentTypeItems
        var subjectTypeItem = blogContentType.ContentTypeItems.FirstOrDefault(cti => cti.Name.ToLower() == "subject");
        var teaserTypeItem = blogContentType.ContentTypeItems.FirstOrDefault(cti => cti.Name.ToLower() == "teasertext");
        
        if (subjectTypeItem == null)
        {
          subjectTypeItem = new ContentTypeItem
          {
            Name = "Subject", 
            ContentTypeId = blogContentType.ContentTypeId
          };
          _context.ContentTypeItems.Add(subjectTypeItem);
          report.AppendLine("? Created Subject ContentTypeItem");
        }
        else
        {
          report.AppendLine($"? Subject ContentTypeItem exists (ID: {subjectTypeItem.ContentTypeItemId})");
        }
        
        if (teaserTypeItem == null)
        {
          teaserTypeItem = new ContentTypeItem
          {
            Name = "TeaserText",
            ContentTypeId = blogContentType.ContentTypeId
          };
          _context.ContentTypeItems.Add(teaserTypeItem);
          report.AppendLine("? Created TeaserText ContentTypeItem");
        }
        else
        {
          report.AppendLine($"? TeaserText ContentTypeItem exists (ID: {teaserTypeItem.ContentTypeItemId})");
        }
        
        await _context.SaveChangesAsync();
        
        // Get all blog posts
        var blogPosts = await _context.Contents
          .Where(c => c.ContentTypeId == blogContentType.ContentTypeId)
          .Include(c => c.ContentItems)
          .ToListAsync();
          
        report.AppendLine($"\nFound {blogPosts.Count} blog posts");
        
        int createdItems = 0;
        
        foreach (var blog in blogPosts)
        {
          report.AppendLine($"\nProcessing: {blog.Title} ({blog.UrlToDisplay})");
          
          // Check if Subject ContentItem exists
          var existingSubject = blog.ContentItems.FirstOrDefault(ci => ci.ContentTypeItemId == subjectTypeItem.ContentTypeItemId);
          if (existingSubject == null)
          {
            var subjectItem = new ContentItem
            {
              ContentId = blog.ContentId,
              ContentTypeItemId = subjectTypeItem.ContentTypeItemId,
              Value = blog.Title ?? $"Blog Post {blog.ContentId}" // Use the blog title as subject
            };
            _context.ContentItems.Add(subjectItem);
            report.AppendLine($"  ? Created Subject: \"{subjectItem.Value}\"");
            createdItems++;
          }
          else
          {
            report.AppendLine($"  - Subject exists: \"{existingSubject.Value}\"");
          }
          
          // Check if TeaserText ContentItem exists  
          var existingTeaser = blog.ContentItems.FirstOrDefault(ci => ci.ContentTypeItemId == teaserTypeItem.ContentTypeItemId);
          if (existingTeaser == null)
          {
            // Generate teaser text from body content
            string teaserText = "Read more..."; // Default
            if (!string.IsNullOrEmpty(blog.Body))
            {
              // Extract first 150 characters of text content, removing HTML tags
              var plainText = System.Text.RegularExpressions.Regex.Replace(blog.Body, "<.*?>", "");
              teaserText = plainText.Length > 150 ? plainText.Substring(0, 150) + "..." : plainText;
            }
            
            var teaserItem = new ContentItem
            {
              ContentId = blog.ContentId,
              ContentTypeItemId = teaserTypeItem.ContentTypeItemId,
              Value = teaserText
            };
            _context.ContentItems.Add(teaserItem);
            report.AppendLine($"  ? Created TeaserText: \"{teaserText.Substring(0, Math.Min(50, teaserText.Length))}...\"");
            createdItems++;
          }
          else
          {
            var preview = existingTeaser.Value?.Substring(0, Math.Min(50, existingTeaser.Value?.Length ?? 0)) ?? "";
            report.AppendLine($"  - TeaserText exists: \"{preview}...\"");
          }
        }
        
        if (createdItems > 0)
        {
          await _context.SaveChangesAsync();
          report.AppendLine($"\n? Successfully created {createdItems} ContentItems!");
        }
        else
        {
          report.AppendLine("\n? All ContentItems already exist, no action needed");
        }
        
        report.AppendLine("\n? Blog ContentItems rebuild complete!");
        report.AppendLine("? Your blog posts should now display Subject and TeaserText correctly");
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Rebuild ContentItems for all content types based on common patterns
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RebuildAllContentItems()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Rebuild All ContentItems Utility ===\n");
      
      try
      {
        var allContentTypes = await _context.ContentTypes
          .Include(ct => ct.ContentTypeItems)
          .ToListAsync();
          
        report.AppendLine($"Found {allContentTypes.Count} content types");
        
        foreach (var contentType in allContentTypes)
        {
          report.AppendLine($"\n--- Processing ContentType: {contentType.Name} ---");
          
          // Ensure basic ContentTypeItems exist based on content type
          var requiredTypeItems = GetRequiredContentTypeItems(contentType.Name.ToLower());
          
          foreach (var requiredItem in requiredTypeItems)
          {
            var existingTypeItem = contentType.ContentTypeItems.FirstOrDefault(cti => cti.Name.ToLower() == requiredItem.ToLower());
            if (existingTypeItem == null)
            {
              var newTypeItem = new ContentTypeItem
              {
                Name = requiredItem,
                ContentTypeId = contentType.ContentTypeId
              };
              _context.ContentTypeItems.Add(newTypeItem);
              report.AppendLine($"  ? Created ContentTypeItem: {requiredItem}");
            }
          }
        }
        
        await _context.SaveChangesAsync();
        
        // Now rebuild ContentItems for each content
        var allContents = await _context.Contents
          .Include(c => c.ContentType)
          .ThenInclude(ct => ct.ContentTypeItems)
          .Include(c => c.ContentItems)
          .ToListAsync();
          
        int totalCreatedItems = 0;
        
        foreach (var content in allContents)
        {
          if (content.ContentType == null) continue;
          
          var requiredItems = GetRequiredContentTypeItems(content.ContentType.Name.ToLower());
          
          foreach (var requiredItemName in requiredItems)
          {
            var typeItem = content.ContentType.ContentTypeItems.FirstOrDefault(cti => cti.Name.ToLower() == requiredItemName.ToLower());
            if (typeItem == null) continue;
            
            var existingContentItem = content.ContentItems.FirstOrDefault(ci => ci.ContentTypeItemId == typeItem.ContentTypeItemId);
            if (existingContentItem == null)
            {
              var newContentItem = new ContentItem
              {
                ContentId = content.ContentId,
                ContentTypeItemId = typeItem.ContentTypeItemId,
                Value = GenerateDefaultValue(requiredItemName, content)
              };
              _context.ContentItems.Add(newContentItem);
              totalCreatedItems++;
            }
          }
        }
        
        if (totalCreatedItems > 0)
        {
          await _context.SaveChangesAsync();
          report.AppendLine($"\n? Successfully created {totalCreatedItems} ContentItems across all content types!");
        }
        else
        {
          report.AppendLine("\n? All ContentItems already exist, no action needed");
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }
    
    private List<string> GetRequiredContentTypeItems(string contentTypeName)
    {
      return contentTypeName.ToLower() switch
      {
        "blog" => new List<string> { "Subject", "TeaserText" },
        "projects" => new List<string> { "Subject", "TeaserText", "ProjectUrl" },
        "games" => new List<string> { "Name", "TeaserText", "Screenshot" },
        "about" => new List<string> { "Subject" },
        _ => new List<string> { "Subject", "TeaserText" } // Default items
      };
    }
    
    private string GenerateDefaultValue(string itemName, Content content)
    {
      return itemName.ToLower() switch
      {
        "subject" => content.Title ?? $"Content {content.ContentId}",
        "name" => content.Title ?? $"Content {content.ContentId}",
        "teasertext" => GenerateTeaserText(content.Body),
        "projecturl" => "#",
        "screenshot" => "/images/placeholder.png",
        _ => $"Default {itemName}"
      };
    }
    
    private string GenerateTeaserText(string? body)
    {
      if (string.IsNullOrEmpty(body)) return "Read more...";
      
      // Remove HTML tags and get first 150 characters
      var plainText = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", "");
      return plainText.Length > 150 ? plainText.Substring(0, 150) + "..." : plainText;
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

    /// <summary>
    /// Restore ContentItems from backup database
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> RestoreContentItemsFromBackup()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Restore ContentItems from Backup Database ===\n");
      
      try
      {
        var backupDbPath = Path.Combine(Directory.GetCurrentDirectory(), "dscms-data", "dscms-backup.db");
        var currentDbPath = Path.Combine(Directory.GetCurrentDirectory(), "dscms-data", "dscms.db");
        
        report.AppendLine($"Backup DB Path: {backupDbPath}");
        report.AppendLine($"Current DB Path: {currentDbPath}");
        
        if (!System.IO.File.Exists(backupDbPath))
        {
          report.AppendLine("ERROR: Backup database file not found!");
          report.AppendLine($"Please ensure the backup file exists at: {backupDbPath}");
          return Content(report.ToString(), "text/plain");
        }
        
        report.AppendLine("? Backup database file found");
        
        // Create connection string for backup database
        var backupConnectionString = $"Data Source={backupDbPath}";
        
        using var backupConnection = new Microsoft.Data.Sqlite.SqliteConnection(backupConnectionString);
        await backupConnection.OpenAsync();
        
        // Get ContentTypeItems from backup
        var backupContentTypeItems = new List<(int Id, string Name, int ContentTypeId)>();
        using (var cmd = backupConnection.CreateCommand())
        {
          cmd.CommandText = "SELECT ContentTypeItemId, Name, ContentTypeId FROM ContentTypeItems";
          using var reader = await cmd.ExecuteReaderAsync();
          while (await reader.ReadAsync())
          {
            backupContentTypeItems.Add((
              reader.GetInt32(0), // ContentTypeItemId
              reader.GetString(1), // Name
              reader.GetInt32(2)  // ContentTypeId
            ));
          }
        }
        
        report.AppendLine($"Found {backupContentTypeItems.Count} ContentTypeItems in backup");
        
        // Get ContentItems from backup
        var backupContentItems = new List<(int Id, string Value, int ContentTypeItemId, int ContentId)>();
        using (var cmd = backupConnection.CreateCommand())
        {
          cmd.CommandText = "SELECT ContentItemId, Value, ContentTypeItemId, ContentId FROM ContentItems";
          using var reader = await cmd.ExecuteReaderAsync();
          while (await reader.ReadAsync())
          {
            var value = reader.IsDBNull(1) ? "" : reader.GetString(1); // Value
            backupContentItems.Add((
              reader.GetInt32(0), // ContentItemId
              value,
              reader.GetInt32(2), // ContentTypeItemId
              reader.GetInt32(3)  // ContentId
            ));
          }
        }
        
        report.AppendLine($"Found {backupContentItems.Count} ContentItems in backup");
        
        await backupConnection.CloseAsync();
        
        // Now restore to current database
        int restoredContentTypeItems = 0;
        int restoredContentItems = 0;
        
        // Restore ContentTypeItems first
        foreach (var item in backupContentTypeItems)
        {
          var existingItem = await _context.ContentTypeItems
            .FirstOrDefaultAsync(cti => cti.ContentTypeItemId == item.Id);
            
          if (existingItem == null)
          {
            // Check if ContentType exists
            var contentTypeExists = await _context.ContentTypes
              .AnyAsync(ct => ct.ContentTypeId == item.ContentTypeId);
              
            if (contentTypeExists)
            {
              var newContentTypeItem = new ContentTypeItem
              {
                ContentTypeItemId = item.Id,
                Name = item.Name,
                ContentTypeId = item.ContentTypeId
              };
              
              _context.ContentTypeItems.Add(newContentTypeItem);
              restoredContentTypeItems++;
            }
            else
            {
              report.AppendLine($"WARNING: ContentType {item.ContentTypeId} not found for ContentTypeItem {item.Name}");
            }
          }
        }
        
        await _context.SaveChangesAsync();
        report.AppendLine($"? Restored {restoredContentTypeItems} ContentTypeItems");
        
        // Restore ContentItems
        foreach (var item in backupContentItems)
        {
          var existingItem = await _context.ContentItems
            .FirstOrDefaultAsync(ci => ci.ContentItemId == item.Id);
            
          if (existingItem == null)
          {
            // Check if Content and ContentTypeItem exist
            var contentExists = await _context.Contents
              .AnyAsync(c => c.ContentId == item.ContentId);
              
            var contentTypeItemExists = await _context.ContentTypeItems
              .AnyAsync(cti => cti.ContentTypeItemId == item.ContentTypeItemId);
              
            if (contentExists && contentTypeItemExists)
            {
              var newContentItem = new ContentItem
              {
                ContentItemId = item.Id,
                Value = item.Value,
                ContentTypeItemId = item.ContentTypeItemId,
                ContentId = item.ContentId
              };
              
              _context.ContentItems.Add(newContentItem);
              restoredContentItems++;
            }
            else
            {
              if (!contentExists)
                report.AppendLine($"WARNING: Content {item.ContentId} not found for ContentItem {item.Id}");
              if (!contentTypeItemExists)
                report.AppendLine($"WARNING: ContentTypeItem {item.ContentTypeItemId} not found for ContentItem {item.Id}");
            }
          }
        }
        
        await _context.SaveChangesAsync();
        report.AppendLine($"? Restored {restoredContentItems} ContentItems");
        
        report.AppendLine($"\n=== Restoration Summary ===");
        report.AppendLine($"ContentTypeItems restored: {restoredContentTypeItems}");
        report.AppendLine($"ContentItems restored: {restoredContentItems}");
        
        if (restoredContentItems > 0)
        {
          report.AppendLine("\n? ContentItems successfully restored from backup!");
          report.AppendLine("? Your blog posts should now display Subject and TeaserText correctly");
          report.AppendLine("? Test your blog posts to verify the restoration worked");
        }
        else
        {
          report.AppendLine("\n! No ContentItems needed restoration (they may already exist)");
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
    /// Compare current database with backup to see what's missing
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CompareWithBackup()
    {
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== Compare Current Database with Backup ===\n");
      
      try
      {
        var backupDbPath = Path.Combine(Directory.GetCurrentDirectory(), "dscms-data", "dscms-backup.db");
        
        if (!System.IO.File.Exists(backupDbPath))
        {
          report.AppendLine("ERROR: Backup database file not found!");
          return Content(report.ToString(), "text/plain");
        }
        
        var backupConnectionString = $"Data Source={backupDbPath}";
        
        using var backupConnection = new Microsoft.Data.Sqlite.SqliteConnection(backupConnectionString);
        await backupConnection.OpenAsync();
        
        // Count items in backup
        int backupContentTypeItemCount = 0;
        int backupContentItemCount = 0;
        
        using (var cmd = backupConnection.CreateCommand())
        {
          cmd.CommandText = "SELECT COUNT(*) FROM ContentTypeItems";
          backupContentTypeItemCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        
        using (var cmd = backupConnection.CreateCommand())
        {
          cmd.CommandText = "SELECT COUNT(*) FROM ContentItems";
          backupContentItemCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        
        await backupConnection.CloseAsync();
        
        // Count items in current database
        var currentContentTypeItemCount = await _context.ContentTypeItems.CountAsync();
        var currentContentItemCount = await _context.ContentItems.CountAsync();
        
        report.AppendLine("=== Comparison Results ===");
        report.AppendLine($"ContentTypeItems - Backup: {backupContentTypeItemCount}, Current: {currentContentTypeItemCount}");
        report.AppendLine($"ContentItems - Backup: {backupContentItemCount}, Current: {currentContentItemCount}");
        
        var missingContentTypeItems = backupContentTypeItemCount - currentContentTypeItemCount;
        var missingContentItems = backupContentItemCount - currentContentItemCount;
        
        if (missingContentTypeItems > 0)
          report.AppendLine($"? Missing {missingContentTypeItems} ContentTypeItems");
        else
          report.AppendLine("? All ContentTypeItems present");
          
        if (missingContentItems > 0)
          report.AppendLine($"? Missing {missingContentItems} ContentItems");
        else
          report.AppendLine("? All ContentItems present");
          
        if (missingContentTypeItems > 0 || missingContentItems > 0)
        {
          report.AppendLine("\nRecommendation: Run /Diagnostics/RestoreContentItemsFromBackup to restore missing data");
        }
        else
        {
          report.AppendLine("\n? No restoration needed - all data is present");
        }
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
        report.AppendLine($"Stack trace: {ex.StackTrace}");
      }
      
      return Content(report.ToString(), "text/plain");
    }
  }
}