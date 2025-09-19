using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DSCMS.Data;
using DSCMS.Models;

namespace DSCMS.Controllers
{
  /// <summary>
  /// Controller for diagnostic and maintenance utilities
  /// </summary>
  public class DiagnosticsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public DiagnosticsController(ApplicationDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Safe diagnostic utility to check user-content relationships
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Users()
    {
      var users = await _context.Users.ToListAsync();
      var contents = await _context.Contents.ToListAsync();
      
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS User Relationship Diagnostics ===\n");
      
      report.AppendLine($"Total Users: {users.Count}");
      foreach (var user in users)
      {
        report.AppendLine($"  User: {user.Id} | DisplayName: '{user.DisplayName}' | Email: {user.Email}");
      }
      
      report.AppendLine($"\nTotal Content Items: {contents.Count}");
      var problemContents = new List<Content>();
      
      foreach (var content in contents)
      {
        var hasValidCreatedBy = !string.IsNullOrEmpty(content.CreatedBy) && users.Any(u => u.Id == content.CreatedBy);
        var hasValidLastUpdatedBy = !string.IsNullOrEmpty(content.LastUpdatedBy) && users.Any(u => u.Id == content.LastUpdatedBy);
        
        if (!hasValidCreatedBy || !hasValidLastUpdatedBy)
        {
          problemContents.Add(content);
          report.AppendLine($"  ISSUE - Content {content.ContentId}: CreatedBy='{content.CreatedBy}' ({(hasValidCreatedBy ? "VALID" : "INVALID")}), LastUpdatedBy='{content.LastUpdatedBy}' ({(hasValidLastUpdatedBy ? "VALID" : "INVALID")})");
        }
        else
        {
          var creator = users.FirstOrDefault(u => u.Id == content.CreatedBy);
          report.AppendLine($"  OK - Content {content.ContentId}: Created by '{creator?.DisplayName}' ({content.CreatedBy})");
        }
      }
      
      if (problemContents.Any())
      {
        report.AppendLine($"\n=== PROBLEMS FOUND ===");
        report.AppendLine($"Found {problemContents.Count} content items with invalid user references.");
        report.AppendLine($"To fix these issues, navigate to: /Diagnostics/FixUsers");
      }
      else
      {
        report.AppendLine($"\n=== ALL GOOD ===");
        report.AppendLine($"All content items have valid user references!");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Safe fix utility for user-content relationships
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixUsers()
    {
      var users = await _context.Users.ToListAsync();
      var contents = await _context.Contents.ToListAsync();
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database. Cannot fix content relationships.", "text/plain");
      }
      
      var defaultUser = users.OrderBy(u => u.Id).First(); // Use first user as default
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS User Data Fix Utility ===\n");
      report.AppendLine($"Using default user: {defaultUser.DisplayName} ({defaultUser.Id})\n");
      
      var fixedCount = 0;
      var validUserIds = users.Select(u => u.Id).ToHashSet();
      
      foreach (var content in contents)
      {
        var needsFixing = false;
        var originalCreatedBy = content.CreatedBy;
        var originalLastUpdatedBy = content.LastUpdatedBy;
        
        // Fix CreatedBy if invalid
        if (string.IsNullOrEmpty(content.CreatedBy) || !validUserIds.Contains(content.CreatedBy))
        {
          content.CreatedBy = defaultUser.Id;
          needsFixing = true;
        }
        
        // Fix LastUpdatedBy if invalid
        if (string.IsNullOrEmpty(content.LastUpdatedBy) || !validUserIds.Contains(content.LastUpdatedBy))
        {
          content.LastUpdatedBy = defaultUser.Id;
          needsFixing = true;
        }
        
        if (needsFixing)
        {
          fixedCount++;
          report.AppendLine($"Fixed Content {content.ContentId}: CreatedBy '{originalCreatedBy}' -> '{content.CreatedBy}', LastUpdatedBy '{originalLastUpdatedBy}' -> '{content.LastUpdatedBy}'");
        }
      }
      
      if (fixedCount > 0)
      {
        try
        {
          // Verify the default user still exists before saving
          var userStillExists = await _context.Users.AnyAsync(u => u.Id == defaultUser.Id);
          if (!userStillExists)
          {
            return Content($"ERROR: Default user {defaultUser.Id} no longer exists. Cannot proceed with fix.", "text/plain");
          }
          
          await _context.SaveChangesAsync();
          report.AppendLine($"\nSuccessfully fixed {fixedCount} content items!");
          report.AppendLine($"You can now check the blog at /blog to see the correct author names.");
        }
        catch (Exception ex)
        {
          report.AppendLine($"\nERROR while saving changes: {ex.Message}");
          
          // Additional debugging information
          report.AppendLine($"\nDEBUG INFO:");
          report.AppendLine($"Default user ID: {defaultUser.Id}");
          report.AppendLine($"Default user exists: {await _context.Users.AnyAsync(u => u.Id == defaultUser.Id)}");
          report.AppendLine($"Total users in DB: {await _context.Users.CountAsync()}");
          
          // Check if the user ID format is correct
          var sampleUserIds = await _context.Users.Take(3).Select(u => u.Id).ToListAsync();
          report.AppendLine($"Sample user IDs: {string.Join(", ", sampleUserIds)}");
          
          return Content(report.ToString(), "text/plain");
        }
      }
      else
      {
        report.AppendLine($"No content items needed fixing. All user references are valid.");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Preview what the fix would do without making changes
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PreviewFix()
    {
      var users = await _context.Users.ToListAsync();
      var contents = await _context.Contents.ToListAsync();
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database. Cannot fix content relationships.", "text/plain");
      }
      
      var defaultUser = users.OrderBy(u => u.Id).First();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS User Data Fix Preview (NO CHANGES MADE) ===\n");
      report.AppendLine($"Would use default user: {defaultUser.DisplayName} ({defaultUser.Id})\n");
      
      var fixCount = 0;
      var validUserIds = users.Select(u => u.Id).ToHashSet();
      
      foreach (var content in contents)
      {
        var needsFixing = false;
        var wouldFixCreatedBy = string.IsNullOrEmpty(content.CreatedBy) || !validUserIds.Contains(content.CreatedBy);
        var wouldFixLastUpdatedBy = string.IsNullOrEmpty(content.LastUpdatedBy) || !validUserIds.Contains(content.LastUpdatedBy);
        
        if (wouldFixCreatedBy || wouldFixLastUpdatedBy)
        {
          fixCount++;
          report.AppendLine($"WOULD FIX Content {content.ContentId}:");
          if (wouldFixCreatedBy)
            report.AppendLine($"  CreatedBy: '{content.CreatedBy}' -> '{defaultUser.Id}'");
          if (wouldFixLastUpdatedBy)
            report.AppendLine($"  LastUpdatedBy: '{content.LastUpdatedBy}' -> '{defaultUser.Id}'");
        }
      }
      
      if (fixCount > 0)
      {
        report.AppendLine($"\nWould fix {fixCount} content items.");
        report.AppendLine($"To actually apply these changes, navigate to: /Diagnostics/FixUsers");
      }
      else
      {
        report.AppendLine($"No content items need fixing. All user references are valid.");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Safe fix utility for user-content relationships (batch processing)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixUsersBatch()
    {
      var users = await _context.Users.ToListAsync();
      var contents = await _context.Contents.ToListAsync();
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database. Cannot fix content relationships.", "text/plain");
      }
      
      var defaultUser = users.OrderBy(u => u.Id).First();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS User Data Fix Utility (Batch Mode) ===\n");
      report.AppendLine($"Using default user: '{defaultUser.DisplayName}' ({defaultUser.Id})\n");
      
      var totalFixed = 0;
      var batchSize = 5; // Process in small batches
      var validUserIds = users.Select(u => u.Id).ToHashSet();
      
      var contentsToFix = contents.Where(c => 
        string.IsNullOrEmpty(c.CreatedBy) || !validUserIds.Contains(c.CreatedBy) ||
        string.IsNullOrEmpty(c.LastUpdatedBy) || !validUserIds.Contains(c.LastUpdatedBy)).ToList();
      
      report.AppendLine($"Found {contentsToFix.Count} content items that need fixing.");
      report.AppendLine($"Processing in batches of {batchSize}...\n");
      
      for (int i = 0; i < contentsToFix.Count; i += batchSize)
      {
        var batch = contentsToFix.Skip(i).Take(batchSize).ToList();
        report.AppendLine($"Processing batch {(i / batchSize) + 1} ({batch.Count} items):");
        
        try
        {
          foreach (var content in batch)
          {
            var originalCreatedBy = content.CreatedBy;
            var originalLastUpdatedBy = content.LastUpdatedBy;
            
            // Fix CreatedBy if invalid
            if (string.IsNullOrEmpty(content.CreatedBy) || !validUserIds.Contains(content.CreatedBy))
            {
              content.CreatedBy = defaultUser.Id;
            }
            
            // Fix LastUpdatedBy if invalid
            if (string.IsNullOrEmpty(content.LastUpdatedBy) || !validUserIds.Contains(content.LastUpdatedBy))
            {
              content.LastUpdatedBy = defaultUser.Id;
            }
            
            report.AppendLine($"  Content {content.ContentId}: '{originalCreatedBy}' -> '{content.CreatedBy}', '{originalLastUpdatedBy}' -> '{content.LastUpdatedBy}'");
          }
          
          await _context.SaveChangesAsync();
          totalFixed += batch.Count;
          report.AppendLine($"  ? Batch saved successfully\n");
        }
        catch (Exception ex)
        {
          report.AppendLine($"  ? Error saving batch: {ex.Message}\n");
          
          // Try to reset the context for the failed items
          foreach (var content in batch)
          {
            _context.Entry(content).Reload();
          }
        }
      }
      
      report.AppendLine($"=== SUMMARY ===");
      report.AppendLine($"Successfully fixed {totalFixed} out of {contentsToFix.Count} content items.");
      
      if (totalFixed > 0)
      {
        report.AppendLine($"You can now check the blog at /blog to see the correct author names.");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Check and fix user DisplayName
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixUserDisplayName()
    {
      var users = await _context.Users.ToListAsync();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== User DisplayName Fix Utility ===\n");
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database.", "text/plain");
      }
      
      foreach (var user in users)
      {
        report.AppendLine($"User ID: {user.Id}");
        report.AppendLine($"Email: {user.Email}");
        report.AppendLine($"UserName: {user.UserName}");
        report.AppendLine($"Current DisplayName: '{user.DisplayName}'");
        
        if (string.IsNullOrWhiteSpace(user.DisplayName))
        {
          user.DisplayName = "Brady Prigge";
          report.AppendLine($"? Updated DisplayName to: '{user.DisplayName}'");
        }
        else
        {
          report.AppendLine($"? DisplayName already set");
        }
        report.AppendLine();
      }
      
      try
      {
        await _context.SaveChangesAsync();
        report.AppendLine("Successfully saved user updates!");
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR saving user updates: {ex.Message}");
      }
      
      return Content(report.ToString(), "text/plain");
    }

    /// <summary>
    /// Alternative fix using direct SQL update (bypasses Entity Framework)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FixUsersSQL()
    {
      var users = await _context.Users.ToListAsync();
      
      if (!users.Any())
      {
        return Content("ERROR: No users found in database. Cannot fix content relationships.", "text/plain");
      }
      
      var defaultUser = users.OrderBy(u => u.Id).First();
      var report = new System.Text.StringBuilder();
      report.AppendLine("=== DSCMS User Data Fix Utility (SQL Mode) ===\n");
      report.AppendLine($"Using default user: '{defaultUser.DisplayName}' ({defaultUser.Id})\n");
      
      try
      {
        // First, let's make sure the user has the correct DisplayName
        if (string.IsNullOrWhiteSpace(defaultUser.DisplayName))
        {
          await _context.Database.ExecuteSqlRawAsync(
            "UPDATE AspNetUsers SET DisplayName = 'Brady Prigge' WHERE Id = {0}",
            defaultUser.Id);
          report.AppendLine("? Updated user DisplayName to 'Brady Prigge'");
        }
        
        // Update CreatedBy references
        var updatedCreatedBy = await _context.Database.ExecuteSqlRawAsync(
          "UPDATE Contents SET CreatedBy = {0} WHERE CreatedBy = '1' OR CreatedBy IS NULL OR CreatedBy NOT IN (SELECT Id FROM AspNetUsers)",
          defaultUser.Id);
        
        // Update LastUpdatedBy references
        var updatedLastUpdatedBy = await _context.Database.ExecuteSqlRawAsync(
          "UPDATE Contents SET LastUpdatedBy = {0} WHERE LastUpdatedBy = '1' OR LastUpdatedBy IS NULL OR LastUpdatedBy NOT IN (SELECT Id FROM AspNetUsers)",
          defaultUser.Id);
        
        report.AppendLine($"? Updated {updatedCreatedBy} CreatedBy references");
        report.AppendLine($"? Updated {updatedLastUpdatedBy} LastUpdatedBy references");
        report.AppendLine($"\nFix completed successfully!");
        report.AppendLine($"You can now check the blog at /blog to see 'Brady Prigge' as the author.");
        
      }
      catch (Exception ex)
      {
        report.AppendLine($"ERROR: {ex.Message}");
      }
      
      return Content(report.ToString(), "text/plain");
    }
  }
}