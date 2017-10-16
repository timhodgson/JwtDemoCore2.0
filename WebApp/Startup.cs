using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Config;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace WebApp
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Setup REST client
      services.Configure<RestClientSettings>(Configuration.GetSection(nameof(RestClientSettings)));
      services.AddTransient<IRestClient, RestClientFactory>();

      // setup JWT Token validation
      services.Configure<JwtTokenValidationSettings>(Configuration.GetSection(nameof(JwtTokenValidationSettings)));
      services.AddSingleton<IJwtTokenValidationSettings, JwtTokenValidationSettingsFactory>();

      // Setup JWT Issuer Settings
      services.Configure<JwtTokenIssuerSettings>(Configuration.GetSection(nameof(JwtTokenIssuerSettings)));
      services.AddSingleton<IJwtTokenIssuerSettings, JwtTokenIssuerSettingsFactory>();

      // Setup ClaimPrincipalManager
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddTransient<IClaimPrincipalManager, ClaimPrincipalManager>();

      // Setup Authentication Cookies
      services.Configure<AuthenticationSettings>(Configuration.GetSection(nameof(AuthenticationSettings)));
      services.AddSingleton<IAuthenticationSettings, AuthenticationSettingsFactory>();

      // Setup Policies
      services.AddAuthorization(options =>
      {
        options.AddPolicy("HR Only", policy => policy.RequireRole("HR-Worker"));
        options.AddPolicy("HR-Manager Only", policy => policy.RequireClaim("CeoApproval", "true"));
      });

      // Authorize all controllers
      var authorizePolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

      // get serviceprovider
      var serviceProvider = services.BuildServiceProvider();

      // Create TokenValidation factory with DI priciple
      var authenticationSettings = serviceProvider.GetService<IAuthenticationSettings>();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddCookie(JwtBearerDefaults.AuthenticationScheme,
                  options =>
                  {
                    options.LoginPath = authenticationSettings.LoginPath;
                    options.AccessDeniedPath = authenticationSettings.AccessDeniedPath;
                    options.Events = new CookieAuthenticationEvents
                    {
                      // Check if JWT needs refreshment 
                      OnValidatePrincipal = RefreshTokenMonitor.ValidateAsync
                    };
                  }
                );


      // Add Mvc with options
      services.AddMvc(config =>
      { config.Filters.Add(new AuthorizeFilter(authorizePolicy)); })
         .AddJsonOptions(options =>
         {
           // Override default camelCase style (yes its werd default configuration results in camel case)
           options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
         });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseBrowserLink();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }

      app.UseStaticFiles();

      app.UseAuthentication();

      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
