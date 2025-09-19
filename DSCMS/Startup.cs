using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Services;
using Microsoft.AspNetCore.StaticFiles;

namespace DSCMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => 
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

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            // Updated for .NET 9
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Configure routes using a safer approach
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
                name: "DefaultAdmin",
                pattern: "Admin",
                defaults: new { controller = "Layouts", action = "Index" });

            app.MapControllerRoute(
                name: "cms",
                pattern: "{contentTypeName}/{contentUrl?}",
                defaults: new { controller = "DSCMS", action = "Content" });
            
            // Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=DSCMS}/{action=Content}/{contentTypeName?}/{contentUrl?}");

            app.MapRazorPages();
        }
    }
}
