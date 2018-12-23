using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Profile")]
  public class ProfileController : Controller {
    private readonly ProfileService _profileService;
    private readonly AuthorizationService _authorizationService;

    public ProfileController(ProfileService profileService, AuthorizationService authorizationService) {
      _profileService = profileService;
      _authorizationService = authorizationService;
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username) {
        return View(await _profileService.GetProfileDetailsVm(username, User));
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditAccount(string username) {
      if (_authorizationService.IsAuthorizedForProfileEdit(username, User))
        return View(await _profileService.GetAccountEditVm(User));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAccount(string username, ProfileEditVm profileEditVm) {
      if (!ModelState.IsValid)
        return (View(profileEditVm));

      if (!_authorizationService.IsAuthorizedForProfileEdit(username, User))
        return RedirectToAction("AccessDenied", "Account");

      var result = await _profileService.UpdateProfile(profileEditVm, User);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(profileEditVm);
      }

      return RedirectToAction(nameof(Details));
    }
  }
}
