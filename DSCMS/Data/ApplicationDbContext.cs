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
            
            // Configure the relationship between ContentType and Template
            builder.Entity<ContentType>()
                .HasOne(ct => ct.Template)
                .WithMany(t => t.ContentTypes)
                .HasForeignKey(ct => ct.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ContentType>()
                .HasOne(ct => ct.DefaultContentTemplate)
                .WithMany(t => t.HasAsDefaultContentTemplate)
                .HasForeignKey(ct => ct.DefaultTemplateForContent)
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
        }

        public DbSet<Layout> Layouts { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentTypeItem> ContentTypeItems { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }

    }
}
