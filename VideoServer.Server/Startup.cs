using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.IO;

using VideoServer.Server.Services;
using VideoServer.Shared;
using VideoServer.Shared.Models;


namespace VideoServer.Server
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration config) {
            _configuration = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(_configuration.GetSection(nameof(DatabaseSettings)));
            services.AddSingleton<IDatabaseSettings>(sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            services.Configure<CacheSettings>(_configuration.GetSection(nameof(CacheSettings)));
            services.AddSingleton<ICacheSettings>(sp => sp.GetRequiredService<IOptions<CacheSettings>>().Value);

            services.Configure<VideoSettings>(_configuration.GetSection(nameof(VideoSettings)));
            services.AddSingleton<IVideoSettings>(sp => sp.GetRequiredService<IOptions<VideoSettings>>().Value);

            services.AddSingleton<DbService>();

            services.AddScoped<IVideoService, VideoService>();

            services.AddMemoryCache();
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            app.UseBlazor<Client.Startup>();
        }
    }
}
