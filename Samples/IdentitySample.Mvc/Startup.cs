using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentitySample.Models;
using IdentitySample.Services;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using AspNetCore.Identity.DocumentDb;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System.Net;
using Microsoft.Extensions.Hosting;

namespace IdentitySample
{
    public struct DocumentDbClientConfig
    {
        public string EndpointUri;
        public string AuthenticationKey;
    }

    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add DocumentDb client singleton instance (it's recommended to use a singleton instance for it)
            services.AddDefaultDocumentClientForIdentity(
                Configuration.GetValue<Uri>("DocumentDbClient:EndpointUri"),
                Configuration.GetValue<string>("DocumentDbClient:AuthorizationKey"),
                afterCreation: InitializeDocumentClient);

            // Add framework services.

            services.AddAuthentication();

            services.AddIdentity<ApplicationUser, DocumentDbIdentityRole>()
                .AddDocumentDbStores(options =>
                {
                    options.UserStoreDocumentCollection = "AspNetIdentity";
                    options.Database = "AspNetCoreIdentitySample";
                });

            services.AddMvc(option =>  option.EnableEndpointRouting = false);

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseAuthentication();
            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeDocumentClient(DocumentClient client)
        {
            try
            {
                // Does the DB exist?
                var db = client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("AspNetCoreIdentitySample")).Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex.GetType() == typeof(DocumentClientException) && ((DocumentClientException)ex).StatusCode == HttpStatusCode.NotFound)
                    {
                        // Create DB
                        var db = client.CreateDatabaseAsync(new Database() { Id = "AspNetCoreIdentitySample" }).Result;
                        return true;
                    }

                    return false;
                });
            }

            try
            {
                // Does the Collection exist?
                var collection = client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("AspNetCoreIdentitySample", "AspNetIdentity")).Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex.GetType() == typeof(DocumentClientException) && ((DocumentClientException)ex).StatusCode == HttpStatusCode.NotFound)
                    {
                        DocumentCollection collection = new DocumentCollection() { Id = "AspNetIdentity" };
                        collection = client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri("AspNetCoreIdentitySample"), collection).Result;

                        return true;
                    }

                    return false;
                });
            }
        }
    }
}

