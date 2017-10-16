using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Account;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Controllers;

namespace Security.Controllers
{
  [AllowAnonymous]
  public class AccountController : Controller
  {
    private readonly IClaimPrincipalManager claimPrincipalManager;

    public AccountController(IClaimPrincipalManager claimPrincipalManager)
    {
      this.claimPrincipalManager = claimPrincipalManager;
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
      await claimPrincipalManager.LogoutAsync();

      return RedirectToAction(nameof(HomeController.Index), "Home");
    }


    [HttpGet]
    public IActionResult Login(String returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;
      return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm]LoginViewModel model, String returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;

      if (!ModelState.IsValid)
      {
        ModelState.AddModelError(String.Empty, "Invalid login form");
        return View(model);
      }

      if (await claimPrincipalManager.LoginAsync(model.Email, model.Password))
        return RedirectToLocal(returnUrl);
      else
      {
        ModelState.AddModelError(String.Empty, "Invalid login attempt.");
        return View(model);
      }
    }


    // Prevent session stealing
    private IActionResult RedirectToLocal(String returnUrl)
    {
      if (Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);
      else
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
  }
}
