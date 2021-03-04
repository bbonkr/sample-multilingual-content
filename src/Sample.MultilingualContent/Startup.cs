using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Sample.MultilingualContent.Data;
using Sample.MultilingualContent.Repositories;
using Sample.MultilingualContent.Services;

namespace Sample.MultilingualContent
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
            services.AddOptions<AzureTranslatorConnectionOptions>().Configure(options =>
            {
                Configuration.GetSection(AzureTranslatorConnectionOptions.Name).Bind(options);
            });

            services.AddDbContext<AppDbContext>(builder =>
            {
                var connectionString = Configuration.GetConnectionString("Default");
                builder.UseSqlServer(connectionString);
            });

            services.AddTransient<ILanguageRepository, LanguageRepository>();
            services.AddTransient<IPostRepository, PostRepository>();
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<ITranslatorService, TranslatorService>();

            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample.MultilingualContent", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    //dbContext.Database.EnsureDeleted();
                    dbContext.Database.Migrate();

                    Seeder.Seed(dbContext);
                }

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.MultilingualContent v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapAreaControllerRoute("api", "api", "[area]/[controller]");
                endpoints.MapControllers();
            });
        }
    }
}
