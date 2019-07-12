using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Linq;

namespace BlazorDynamicList.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddNewtonsoftJson();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseClientSideBlazorFiles<Client.Startup>();

            app.UseRouting();

            const string blazorClient = "blazordynamiclistclient";

            ConfigureStatics(app, blazorClient);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                MapBlazorFallback(endpoints, blazorClient);
            });
        }


        private static void ConfigureStatics(IApplicationBuilder app, string projectName)
        {
#if DEBUG
            app.UseStaticFiles();

#else
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/_content/{projectName}")),
                RequestPath = new Microsoft.AspNetCore.Http.PathString("")
            });
#endif
        }

        private static void MapBlazorFallback(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder endpoints, string projectName)
        {
            //This is a workaround for an issue with p6 that creates a different folder
            //structure on publish than the one expected by the blazor fallback
            //for more info go to
            // https://github.com/aspnet/AspNetCore/issues/11185
            string blazorFallbackPath;
#if DEBUG
            blazorFallbackPath = "index.html";
#else
            blazorFallbackPath = $"_content/{projectName}/index.html";
#endif
            endpoints.MapFallbackToClientSideBlazor<Client.Startup>(blazorFallbackPath);
        }
    }
}
