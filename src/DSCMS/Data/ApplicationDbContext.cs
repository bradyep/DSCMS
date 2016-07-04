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
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        public DbSet<Layout> Layouts { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<ContentTypeItem> ContentTypeItems { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
    }
}
