using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DSCMS.Models;

namespace DSCMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure the relationship between ContentType and Template for multiple contents listing
            builder.Entity<ContentType>()
                .HasOne(ct => ct.MultipleContentsTemplate)
                .WithMany(t => t.UsedAsMultipleContentsTemplate)
                .HasForeignKey(ct => ct.MultipleContentsTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship for default single content template
            builder.Entity<ContentType>()
                .HasOne(ct => ct.DefaultSingleContentTemplate)
                .WithMany(t => t.UsedAsDefaultSingleContentTemplate)
                .HasForeignKey(ct => ct.DefaultSingleContentTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Content and ApplicationUser relationships
            builder.Entity<Content>()
                .HasOne(c => c.CreatedByUser)
                .WithMany(u => u.CreatedContent)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Content>()
                .HasOne(c => c.LastUpdatedByUser)
                .WithMany(u => u.UpdatedContent)
                .HasForeignKey(c => c.LastUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Seed SourceTypes data
            builder.Entity<SourceType>().HasData(
                new SourceType { SourceTypeId = 1, Description = "RazorFile" },
                new SourceType { SourceTypeId = 2, Description = "InlineRazor" },
                new SourceType { SourceTypeId = 3, Description = "Markdown" },
                new SourceType { SourceTypeId = 4, Description = "HTML" },
                new SourceType { SourceTypeId = 5, Description = "Text" }
            );
        }

        public DbSet<Layout> Layouts { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentTypeField> ContentTypeFields { get; set; }
        public DbSet<ContentTypeFieldItem> ContentTypeFieldItems { get; set; }
        public DbSet<SourceType> SourceTypes { get; set; }

    }
}
