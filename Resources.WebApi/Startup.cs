using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Config;
using Microsoft.EntityFrameworkCore;
using Entities;
using Resources.Service;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Resources.WebApi
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
      // get environment
      var env = services.BuildServiceProvider().GetRequiredService<IHostingEnvironment>();

      // Get Database connection config
      var connectionString = Configuration.GetConnectionString("DbConnection");

      // Get Driver config
      var databaseDriver = Configuration.GetConnectionString("DatabaseDriver");

      // Setup Database Service layer used in ResourceService
      if (databaseDriver.EqualsEx("MySql"))
        services.AddDbContext<EntityContext>(options => options.UseMySql(connectionString));

      else if (databaseDriver.EqualsEx("InMemory"))
        services.AddDbContext<EntityContext>(options =>
               options.UseInMemoryDatabase("EmployeesDemo")
       // Suppress not supported Transaction Exceptions
       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning)));

      else
        // Fall back to SqlServer
        services.AddDbContext<EntityContext>(options => options.UseSqlServer(connectionString));

      // Setup ResourceService
      services.AddTransient<ICustomerResourceService, EmployeeResourceService>();
      // Setup IIdentity used for simple audit trail in ICustomerResourceService 
      services.AddTransient<IIdentity, IdentityResolverService>();

      // setup JWT Token validation
      services.Configure<JwtTokenValidationSettings>(Configuration.GetSection(nameof(JwtTokenValidationSettings)));
      services.AddSingleton<IJwtTokenValidationSettings, JwtTokenValidationSettingsFactory>();

      // Register the Swagger generator with JWT support
      services.AddSwaggerGen(c =>
      {
        // Tweak for JWT support
        c.AddSecurityDefinition("Bearer", new Swashbuckle.AspNetCore.Swagger.ApiKeyScheme()
        {
          Description = "Authorization format : Bearer {token}",
          Name = "Authorization",
          In = "header",
          Type = "apiKey"
        });

        c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Resources  Api", Version = "v1" });
      });

      // Create TokenValidation factory with DI priciple
      var tokenValidationSettings = services.BuildServiceProvider().GetService<IJwtTokenValidationSettings>();

      // todo
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = tokenValidationSettings.CreateTokenValidationParameters();
          options.SaveToken = true;
          // todo options.RequireHttpsMetadata = false;
        });


      // some samples (this section must contain all the authorization policies used anywhere in the application)
      services.AddAuthorization(options =>
      {
        options.AddPolicy("HR Only", policy => policy.RequireRole("HR-Worker"));
        options.AddPolicy("HR-Manager Only", policy => policy.RequireClaim("CeoApproval", "true"));
      });

      // Secure all controllers by default
      var authorizePolicy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

      // Add Mvc with options
      services.AddMvc(config => { config.Filters.Add(new AuthorizeFilter(authorizePolicy)); }
      )
      // Override default camelCase style (yes its strange the default configuration results in camel case)
      .AddJsonOptions(options =>
      {
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        //c.ShowRequestHeaders();
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resources Api v1");
      });

      app.UseAuthentication();

      app.UseMvc();
    }
  }
}
