using System;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [AllowAnonymous]
  [RequireHttps]
  [Route("Account")]
  public class AccountController : Controller {
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) {
      _accountService = accountService;
    }

    [Route("Register")]
    [HttpGet]
    public async Task<IActionResult> Register() {
      return View();
    }

    [Route("Register")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model) {
      if (!ModelState.IsValid)
        return View(model);
      var result = await _accountService.Add(model);


      if (result.Succeeded)
        return RedirectToAction(nameof(Login));

      foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

      return View(model);
    }

    [Route("Login")]
    [HttpGet]
    public IActionResult Login(string returnUrl = null) {
      var model = new LoginVm {ReturnUrl = returnUrl};
      return View(model);
    }


    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model) {

      if (!ModelState.IsValid)
        return View(model);

      var result = await _accountService.Login(model);

      if (result.Succeeded)
        return Redirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : "/");

      ModelState.AddModelError(string.Empty, "Invalid login attempt.");

      return View(model);
    }

    [HttpGet]
    [Route("AccessDenied")]
    public IActionResult AccessDenied(string returnUrl = null) {
      return View();
    }

    [Route("Logout")]
    [HttpPost]
    public async Task<IActionResult> Logout() {
      await _accountService.SignOut();
      return Redirect("/");
    }
  }
}