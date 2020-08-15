using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThemeController
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Serves the folders in the "StaticFolders" section (the theme store and the two sites),
            // so everything runs from one address without IIS. Leave the section empty when IIS serves them.
            foreach (var folder in Configuration.GetSection("StaticFolders").GetChildren())
            {
                string path = Path.GetFullPath(Path.Combine(env.ContentRootPath, folder.Value));
                if (Directory.Exists(path))
                {
                    app.UseFileServer(new FileServerOptions()
                    {
                        RequestPath = "/" + folder.Key,
                        FileProvider = new PhysicalFileProvider(path)
                    });
                }
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
