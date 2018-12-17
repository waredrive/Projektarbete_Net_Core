using System;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {

  [AllowAnonymous]
  [RequireHttps]
  [Route("account")]
  public class AccountController : Controller {
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) {
      _accountService = accountService;
    }

    [Route("register")]
    [HttpGet]
    public async Task<IActionResult> Register() {
      var password = Guid.NewGuid() + "A!";
      var result = await _accountService.Add(new RegisterViewModel {
        Birthdate = DateTime.Now,
        Password = password,
        ConfirmPassword = password,
        Email = "test@test.com",
        FirstName = "TestName",
        LastName = "TestSurname",
        UserName = Guid.NewGuid().ToString()
      });

      return View();
    }

    [Route("register")]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model) {
      if (!ModelState.IsValid)
        return View(model);
      var result = await _accountService.Add(model);


      if (result.Succeeded)
        return RedirectToAction(nameof(Login));

      foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

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
      if (!ModelState.IsValid)
        return View(model);

      var result = await _accountService.Login(model);

      if (result.Succeeded)
        return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");

      ModelState.AddModelError(string.Empty, "Invalid login attempt.");

      return View(model);
    }

    [HttpGet]
    public IActionResult AccessDenied() {
      return View();
    }

    [Route("logout")]
    [HttpPost]
    public async Task<IActionResult> Logout() {
      await _accountService.SignOut();
      return Redirect("/");
    }
  }
}