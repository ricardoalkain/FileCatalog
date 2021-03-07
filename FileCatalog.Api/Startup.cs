using System;
using FileCatalog.Respositories;
using FileCatalog.Respositories.Database;
using FileCatalog.Respositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FileCatalog.App
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
            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("beta", new OpenApiInfo
                {
                    Version = "beta",
                    Title = "An Awesome File Catalog API",
                    Description = "Mess around with files (PDF only for now, sorry for that)",
                    Contact = new OpenApiContact
                    {
                        Name = "Rick Alves",
                        Email = "ricardo.alkain@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/ricardo-alkain/"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "License",
                        Url = new Uri("https://opensource.org/licenses/MIT"),
                    }
                });
                c.IncludeXmlComments(@"bin\FileCatalog.Api.xml");
            });

            services.AddScoped<IFileRepository, FileRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/beta/swagger.json", "Awesome API beta");
                c.RoutePrefix = string.Empty; // As Swagger is being used as our UI, let's make it our home page
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            /*
             * NOTE: This method is intended to populate DB so we have something to start with.
             * Refer to its implementation for more notes.
             */

            app.BuildDatabase(Configuration);
        }
    }
}
