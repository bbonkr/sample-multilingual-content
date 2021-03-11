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


using kr.bbon.AspNetCore;
using kr.bbon.Azure.Translator.Services;
using kr.bbon.Azure.Translator.Services.Strategies;
using Sample.MultilingualContent.Domains;

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
            services.Configure<AppOptions>(Configuration.GetSection(AppOptions.Name));
            services.Configure<AppOptions>(Configuration.GetSection(AppOptions.Name));
            services.Configure<AzureTranslatorConnectionOptions>(Configuration.GetSection(AzureTranslatorConnectionOptions.Name));
            services.Configure<AzureStorageOptions>(Configuration.GetSection(AzureStorageOptions.Name));


            services.AddDbContext<AppDbContext>(builder =>
            {
                var connectionString = Configuration.GetConnectionString("Default");
                builder.UseSqlServer(connectionString);
            });

            services.AddTransient<ILanguageRepository, LanguageRepository>();
            services.AddTransient<IPostRepository, PostRepository>();
            services.AddTransient<IBookRepository, BookRepository>();

            services.AddTransient<IStorageService<TranslateAzureBlobStorageContainer>, AzureBlobStorageService<TranslateAzureBlobStorageContainer>>();
            services.AddTransient<ITextTranslatorService, TextTranslatorService>();
            services.AddTransient<IDocumentTranslationService, DocumentTranslationService>();

            services.AddTransient<IPostsDomain, PostsDomain>();

            services.AddTransient<ITranslatedDocumentNamingStrategy, TranslatedDocumentNamingStrategy>();

            var defaultVersion = new ApiVersion(1, 1);

            services.AddControllers();

            services.AddApiVersioningAndSwaggerGen<ConfigureSwaggerOptions>(defaultVersion);
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
                app.UseSwaggerUIWithApiVersioning();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
