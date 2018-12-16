using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.AccountViewModels;
using Forum.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers
{
  [RequireHttps]
  [Route("account")]
  public class AccountController : Controller {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    [Route("register")]
    [HttpGet]
    public IActionResult Register() {
      return View();
    }

    [Route("register")]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model) {
      if (ModelState.IsValid) {
        var user = new IdentityUser() {
          Email = model.Email,
          UserName = model.Email
        };

        //if (!string.IsNullOrEmpty(model.FacultyNumber)) {
        //  user.Claims.Add(new IdentityUserClaim<string> {
        //    ClaimType = "FacultyNumber",
        //    ClaimValue = model.FacultyNumber
        //  });
        //}

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded) {
          return RedirectToAction("Login", "Account");
        } else {
          foreach (var error in result.Errors) {
            ModelState.AddModelError(string.Empty, error.Description);
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

  [Route("login")]
  [HttpGet]
    public IActionResult Login() {
      return View();
    }

[Route("login")]
  [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) {
      ViewData["ReturnUrl"] = returnUrl;
      if (ModelState.IsValid) {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded) {
          if (Url.IsLocalUrl(returnUrl)) {
            return Redirect(returnUrl);
          } else {
            //return RedirectToAction(nameof(StudentController.Index), "Student");
          }

        } else {
          ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    [Route("logout")]
    [HttpGet]
    public IActionResult AccessDenied() {
      return View();
    }

    [Route("logout")]

    [HttpPost]
    public async Task<IActionResult> Logout() {
      await _signInManager.SignOutAsync();
      return Redirect("/");
    }


  }
}