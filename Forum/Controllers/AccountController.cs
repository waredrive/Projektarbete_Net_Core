using System.Threading.Tasks;
using Forum.MVC.Models.AccountViewModels;
using Forum.MVC.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers {

  [RequireHttps]
  [Route("account")]
  public class AccountController : Controller {
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) {
      _accountService = accountService;
    }

    [AllowAnonymous]
    [Route("register")]
    [HttpGet]
    public IActionResult Register() {
      return View();
    }

    [AllowAnonymous]
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

    [AllowAnonymous]
    [Route("login")]
    [HttpGet]
    public IActionResult Login() {
      return View();
    }

    [AllowAnonymous]
    [Route("login")]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model) {
      if (!ModelState.IsValid)
        return View(model);

      var result = await _accountService.Login(model);

      if (result.Succeeded)
        return Redirect("/");

      ModelState.AddModelError(string.Empty, "Invalid login attempt.");

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
      await _accountService.SignOut();
      return Redirect("/");
    }
  }
}