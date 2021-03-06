using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using System;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using ImageGallery.Client.HttpHandlers;

namespace ImageGallery.Client
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
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);


            services.AddAuthorization(options=>
            {//This replaces role-based authorization. This allow more complex rules i.e
               /*
                A user on can order a frame of a picture if it is authenticated, it is from belgium or mexico and 
               its suscription level is payinguser
                */
                options.AddPolicy(
                    "CanOrderFrame",
                    policyBuilder=>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("country","be","mx");
                        policyBuilder.RequireClaim("suscriptionlevel","PayingUser");
                    });
            });

            //our BearerTokenHandler requires AddHttpContextAccessor so we register it
            services.AddHttpContextAccessor();
            //Transient-very short-live service
            services.AddTransient<BearerTokenHandler>();

            // create an HttpClient used for accessing the API
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>();
            //create an HttpClient used for accesing the IDP}
            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,options=>
                {
                    options.AccessDeniedPath = "/Authorization/AccessDenied";
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                 {
                     options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                     options.Authority = "https://localhost:5001/";
                     options.ClientId = "imagegalleryclient";
                     options.ResponseType = "code";
                     options.UsePkce = true;
                     //    options.CallbackPath = new PathString();
                   //  options.SignedOutCallbackPath =  also has a default
                     options.Scope.Add("openid");
                     options.Scope.Add("profile");
                     options.Scope.Add("address");//Una cosa es el scop address y otra el claim address, al agregar el scope address 
                     //decimos que la app tendra acceso a todos los claims de ese scope
                     options.Scope.Add("roles");
                     options.Scope.Add("imagegalleryapi");
                     options.Scope.Add("country");
                     options.Scope.Add("suscriptionlevel");
                     options.Scope.Add("offline_access");
                     //Map claims that you want to include on claims identity
                     options.ClaimActions.MapUniqueJsonKey("role","role");
                     options.ClaimActions.MapUniqueJsonKey("country", "country");
                     options.ClaimActions.MapUniqueJsonKey("suscriptionlevel", "suscriptionlevel");

                     options.SaveTokens = true;
                     options.ClientSecret = "secret";
                     options.GetClaimsFromUserInfoEndpoint = true;
                    // options.ClaimActions.Remove("nbf");//Remover cliam de lista de cliams no mostrados por default
                     options.ClaimActions.DeleteClaim("sid");//No mostrar claims 
                     options.ClaimActions.DeleteClaim("idp");
                     options.ClaimActions.DeleteClaim("s_hash");
                     options.ClaimActions.DeleteClaim("auth_time");

                     // options.ClaimActions.DeleteClaim("address");//No hay necesidad de borrarlo porque no esta incluido por default y no queremos
                     //que en la cookie que ecnripta la infor del identity token se tenga toda esa info, ya que se puede llamar aparte con el userinfo endpoint

                     //we need to tel the framework where it cand find the user's role
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         NameClaimType = JwtClaimTypes.GivenName,
                         RoleClaimType = JwtClaimTypes.Role
                     };
                 });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
