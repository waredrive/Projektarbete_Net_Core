﻿using System.Threading.Tasks;
using Forum.Extensions;
using Forum.Models.Services;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [RequireHttps]
  [Route("Account")]
  public class AccountController : Controller {
    private readonly AccountService _accountService;
    private readonly AuthorizationService _authorizationService;
    private readonly SharedService _sharedService;

    public AccountController(AccountService accountService, AuthorizationService authorizationService,
      SharedService sharedService) {
      _accountService = accountService;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    [AllowAnonymous]
    [Route("Register")]
    [HttpGet]
    public IActionResult Register() {
      return View(_accountService.GetAccountRegisterVm());
    }

    [AllowAnonymous]
    [Route("Register")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AccountRegisterVm accountRegisterVm) {
      if (!ModelState.IsValid)
        return View(accountRegisterVm);

      var ageValidationResult = _accountService.HasMinimumAllowedAge(accountRegisterVm.Birthdate);

      if (!ageValidationResult.Success) {
        foreach (var error in ageValidationResult.Errors)
          ModelState.AddModelError(nameof(accountRegisterVm.Birthdate), error);
        return View(accountRegisterVm);
      }

      var result = await _accountService.AddAsync(accountRegisterVm);

      if (result.Succeeded)
        return RedirectToAction(nameof(Login));

      foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);
      TempData["ModalHeader"] = "Success";
      TempData["ModalMessage"] = "Your Account has been registered!";
      return View(accountRegisterVm);
    }

    [AllowAnonymous]
    [Route("Login")]
    [HttpGet]
    public IActionResult Login(string returnUrl = null) {
      if (User.Identity.IsAuthenticated) {
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }
      return View(new AccountLoginVm { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [Route("Login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AccountLoginVm accountLoginVm) {
      if (!ModelState.IsValid)
        return View(accountLoginVm);

      var result = await _accountService.LoginAsync(accountLoginVm);
      if (result.Succeeded) {
        var returnUrl = Url.IsLocalUrl(accountLoginVm.ReturnUrl) ? accountLoginVm.ReturnUrl : "/";
        returnUrl += $"?returnUrl={returnUrl}";
        return Redirect(returnUrl);
      }
      ModelState.AddModelError(string.Empty, "Invalid login attempt.");

      return View(accountLoginVm);
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditAccount(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Account does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction(nameof(AccessDenied));

      return View(await _accountService.GetAccountEditVmAsync(User));
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(string username, AccountEditVm accountEditVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Account does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(accountEditVm);

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction(nameof(AccessDenied));

      var ageValidationResult = _accountService.HasMinimumAllowedAge(accountEditVm.Birthdate);

      if (!ageValidationResult.Success) {
        foreach (var error in ageValidationResult.Errors)
          ModelState.AddModelError(nameof(accountEditVm.Birthdate), error);
        return View(accountEditVm);
      }

      var result = await _accountService.UpdateAccountAsync(accountEditVm, User);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(accountEditVm);
      }
      TempData.ModalSuccess("Your Account has been updated!");
      return RedirectToAction(nameof(Details), new { returnUrl = ViewBag.ReturnUrl });
    }

    [Route("Update/Password/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditPassword(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Account does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction(nameof(AccessDenied));

      return View(new AccountPasswordEditVm() { Username = username });
    }

    [Route("Update/Password/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPassword(string username, AccountPasswordEditVm accountPasswordEditVm,
      string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Account does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(accountPasswordEditVm);

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction(nameof(AccessDenied));

      var result = await _accountService.UpdatePassword(accountPasswordEditVm, User);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(accountPasswordEditVm);
      }

      TempData.ModalSuccess("Your password has been updated!");
      return RedirectToAction(nameof(Details), new { username });
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username, string returnUrl = null, bool fromProfile = false) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      ViewBag.FromProfile = fromProfile;

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Account does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForAccountDetailsViewAsync(username, User))
        return RedirectToAction(nameof(AccessDenied));

      return View(await _accountService.GetAccountDetailsVmAsync(username, User));
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("AccessDenied")]
    public IActionResult AccessDenied() {
      ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();
      TempData.ModalNoPermission();
      return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
    }

    [Route("Logout")]
    [HttpPost]
    public async Task<IActionResult> Logout() {
      await _accountService.SignOut();
      return Redirect("/");
    }
  }
}