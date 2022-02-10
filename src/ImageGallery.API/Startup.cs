using AutoMapper;
using IdentityServer4.AccessTokenValidation;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

namespace ImageGallery.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                     .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            //Solo aplicaciones clientes con el token emitido por nuestro IDP IdentityServer podran acceder a lo que requiera autorizacion en nuestra api
            services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = "https://localhost:5001/";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        });

            //Esto es para verificar que el IDP valido que el cliente tiene acceso a ese scope
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "imagegalleryclient");
                });
            });


            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            services.AddDbContext<GalleryContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
            });

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();

            // register AutoMapper-related services
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError; ;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
                // The default HSTS value is 30 days. You may want to change this for 
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
