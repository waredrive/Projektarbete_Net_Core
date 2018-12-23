using System;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [RequireHttps]
  [Route("Account")]
  public class AccountController : Controller {
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService) {
      _accountService = accountService;
    }

    [AllowAnonymous]
    [Route("Register")]
    [HttpGet]
    public IActionResult Register() {
      return View();
    }

    [AllowAnonymous]
    [Route("Register")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AccountRegisterVm accountRegisterVm) {
      if (!ModelState.IsValid)
        return View(accountRegisterVm);
      var result = await _accountService.Add(accountRegisterVm);


      if (result.Succeeded)
        return RedirectToAction(nameof(Login));

      foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

      return View(accountRegisterVm);
    }

    [AllowAnonymous]
    [Route("Login")]
    [HttpGet]
    public IActionResult Login(string returnUrl = null) {
      return View(new AccountLoginVm { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AccountLoginVm accountLoginVm) {

      if (!ModelState.IsValid)
        return View(accountLoginVm);

      var result = await _accountService.Login(accountLoginVm);

      if (result.Succeeded)
        return Redirect(Url.IsLocalUrl(accountLoginVm.ReturnUrl) ? accountLoginVm.ReturnUrl : "/");

      ModelState.AddModelError(string.Empty, "Invalid login attempt.");

      return View(accountLoginVm);
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditAccount(string username) {
      if (_accountService.IsAuthorizedForAccountAndPasswordEdit(username, User))
        return View(await _accountService.GetAccountEditVm(User));

      return RedirectToAction(nameof(AccessDenied));
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(string username, AccountEditVm accountEditVm) {
      if (!ModelState.IsValid)
        return (View(accountEditVm));

      if (!_accountService.IsAuthorizedForAccountAndPasswordEdit(username, User))
        return RedirectToAction(nameof(AccessDenied));

      var result = await _accountService.UpdateAccount(accountEditVm, User);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(accountEditVm);
      }

      return RedirectToAction(nameof(Details));
    }

    [Route("Update/Password/{username}")]
    [HttpGet]
    public IActionResult EditPassword(string username) {
      if (_accountService.IsAuthorizedForAccountAndPasswordEdit(username, User))
        return View();

      return RedirectToAction(nameof(AccessDenied));
    }

    [Route("Update/Password/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPassword(string username, AccountPasswordEditVm accountPasswordEditVm) {
      if (!ModelState.IsValid)
        return (View(accountPasswordEditVm));

      if (!_accountService.IsAuthorizedForAccountAndPasswordEdit(username, User))
        return RedirectToAction(nameof(AccessDenied));

      var result = await _accountService.UpdatePassword(accountPasswordEditVm, User);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(accountPasswordEditVm);
      }

      return RedirectToAction(nameof(Details));
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username) {
      if (_accountService.IsAuthorizedForAccountDetailsView(username, User))
        return View(await _accountService.GetAccountDetailsVm(username, User));

      return RedirectToAction(nameof(AccessDenied));
    }

    [AllowAnonymous]
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