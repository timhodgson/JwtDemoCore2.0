using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Security.Models;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Builder
{

  public static class InitDbExtensions
  {
    public static IApplicationBuilder InitDb(this IApplicationBuilder app)
    {
      var roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
      var userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();

      if (userManager.Users.Count() == 0)
      {
        Task.Run(() => InitRoles(roleManager)).Wait();
        Task.Run(() => InitUsers(userManager)).Wait();
      }

      return app;
    }

    private static async Task InitRoles(RoleManager<IdentityRole> roleManager)
    {
      var role = new IdentityRole("Employee");
      await roleManager.CreateAsync(role);

      role = new IdentityRole("HR-Worker");
      await roleManager.CreateAsync(role);
      await roleManager.AddClaimAsync(role, new Claim("Department", "HR"));

      role = new IdentityRole("HR-Manager");
      await roleManager.CreateAsync(role);
      await roleManager.AddClaimAsync(role, new Claim("Department", "HR"));
    }

    private static async Task InitUsers(UserManager<ApplicationUser> userManager)
    {
      var user = new ApplicationUser() { UserName = "employee", Email = "employee@xyz.com" };
      await userManager.CreateAsync(user, "password");
      await userManager.AddToRoleAsync(user, "Employee");

      user = new ApplicationUser() { UserName = "hrworker", Email = "hrworker@xyz.com" };
      await userManager.CreateAsync(user, "password");
      await userManager.AddToRoleAsync(user, "Employee");
      await userManager.AddToRoleAsync(user, "HR-Worker");

      user = new ApplicationUser() { UserName = "hrmanager", Email = "hrmanager@xyz.com" };
      await userManager.CreateAsync(user, "password");
      await userManager.AddToRoleAsync(user, "Employee");
      await userManager.AddToRoleAsync(user, "HR-Worker");
      await userManager.AddToRoleAsync(user, "HR-Manager");

      await userManager.AddClaimAsync(user, new Claim("CeoApproval", "true"));
    }
  }
}
