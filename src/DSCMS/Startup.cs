using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSCMS.Data;
using DSCMS.Models;
using DSCMS.Services;
using Microsoft.AspNetCore.Http;

namespace DSCMS
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Filename=./dscms.db"));

            //services.Configure<CookieAuthenticationOptions>(options =>
            //{
            //    options.LoginPath = new PathString("/Admin/Account/Login");
            //    // options.LogoutPath = new PathString("/Admin/Account/LogOff");
            //});

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Cookies.ApplicationCookie.LoginPath = "/Admin/Account/Login";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password = new Microsoft.AspNetCore.Identity.PasswordOptions
                {
                    RequireNonAlphanumeric = false
                };
            });

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            //app.UseCookieAuthentication(options =>
            //{
            //    options.LoginPath = new PathString("/Admin/Account/Login")
            //});

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
                    name: "cms",
                    template: "{contentTypeName}/{contentUrl?}",
                    defaults: new { controller = "DSCMS", action = "Content" });
                // Default route. Let an admin somehow decide where this takes the user. 
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{contentTypeName?}/{contentUrl?}");
            });
        }
    }
}
