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
                // options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                // Use the option below to test locally on my Windows machine
                options.UseSqlite("Filename=dscms-data/dscms.db"));
                // Use the option below when deploying to production
                // options.UseSqlite("Filename=/dscms-data/dscms.db"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //    name: "blank",
                //    template: "",
                //    defaults: new { controller = "Home", action = "Index" });

                /*
                routes.MapRoute(
                    name: "Admin",
                    template: "Admin/{controller}/{action?}/{id?}",
                    defaults: new { action = "Index" });
                */

                routes.MapRoute(
                    name: "Account",
                    template: "Admin/Account/{action?}/{id?}",
                    defaults: new { controller = "Account", action = "Index" });

                routes.MapRoute(
                    name: "Layouts",
                    template: "Admin/Layouts/{action?}/{id?}",
                    defaults: new { controller = "Layouts", action = "Index" });
                routes.MapRoute(
                    name: "Templates",
                    template: "Admin/Templates/{action?}/{id?}",
                    defaults: new { controller = "Templates", action = "Index" });
                routes.MapRoute(
                    name: "Contents",
                    template: "Admin/Contents/{action?}/{id?}",
                    defaults: new { controller = "Contents", action = "Index" });
                routes.MapRoute(
                    name: "Users",
                    template: "Admin/Users/{action?}/{id?}",
                    defaults: new { controller = "Users", action = "Index" });
                routes.MapRoute(
                    name: "ContentTypes",
                    template: "Admin/ContentTypes/{action?}/{id?}",
                    defaults: new { controller = "ContentTypes", action = "Index" });
                routes.MapRoute(
                    name: "ContentItems",
                    template: "Admin/ContentItems/{action?}/{id?}",
                    defaults: new { controller = "ContentItems", action = "Index" });
                routes.MapRoute(
                    name: "ContentTypeItems",
                    template: "Admin/ContentTypeItems/{action?}/{id?}",
                    defaults: new { controller = "ContentTypeItems", action = "Index" });

                routes.MapRoute(
                    name: "DefaultAdmin",
                    template: "Admin",
                    defaults: new { controller = "Layouts", action = "Index" });

                routes.MapRoute(
                      name: "cms",
                      template: "{contentTypeName}/{contentUrl?}",
                      defaults: new { controller = "DSCMS", action = "Content" });
                // Default route. Let an admin somehow decide where this takes the user. 
                routes.MapRoute(
                    name: "default",
                    template: "{controller=DSCMS}/{action=Content}/{contentTypeName?}/{contentUrl?}");
            });

        }
    }
}
