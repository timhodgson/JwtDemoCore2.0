﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Security.Data;
using Security.Models;
using Security.Services;
using Swashbuckle.AspNetCore.Swagger;
using System.Security.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Security
{
  public class Startup
  {
    public IConfigurationRoot Configuration { get; }

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // get environment
      var env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();

      // Add framework services.
      if (env.IsProduction() || env.IsStaging())
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
      else
       services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("JwtServer"));

      services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
          options.Password.RequireDigit = false;
          options.Password.RequiredLength = 4;
          options.Password.RequireLowercase = false;
          options.Password.RequireUppercase = false;
          options.Password.RequireNonAlphanumeric = false;
        })
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      // Add Mvc with options
      services.AddMvc()
        // Override default camelCase style (yes its strange the default configuration results in camel case)
        .AddJsonOptions(options =>
        {
          options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
        });

      // Register the Swagger generator, defining one or more Swagger documents
      services.AddSwaggerGen(c =>
      {
        c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
        {
          Description = "Authorization format : Bearer {token}",
          Name = "Authorization",
          In = "header",
          Type = "apiKey"
        });

        c.SwaggerDoc("v1", new Info { Title = "Security Api", Version = "v1" });
      });

      // Add application services.
      services.AddTransient<IEmailSender, AuthMessageSender>();

      // setup JWT parameters
      services.Configure<JwtIssuerSettings>(Configuration.GetSection(nameof(JwtIssuerSettings)));
      services.AddTransient<IJwtIssuerOptions, JwtIssuerFactory>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
        app.UseBrowserLink();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");
      }

      // Fill empty inmemory database during development
      if (env.IsDevelopment())
        app.InitDb();

      app.UseStaticFiles();

      app.UseAuthentication();

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Security Api v1");
      });

      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}");
      });
    }
  }
}
