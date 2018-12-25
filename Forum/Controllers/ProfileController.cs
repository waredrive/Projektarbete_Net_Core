using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Extensions;
using Forum.Models.Services;
using Forum.Models.ViewModels.ComponentViewModels.AdminProfileEditViewModels;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Http;
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
      if (!_profileService.DoesProfileExist(username)) {
        return NotFound();
      }

      return View(await _profileService.GetProfileDetailsVm(username, User));
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> Edit(string username) {
      if (!_profileService.DoesProfileExist(username)) {
        return NotFound();
      }

      if (await _authorizationService.IsAuthorizedForAccountAndProfileEdit(username, User))
        return View(await _profileService.GetProfileEditVm(username));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, ProfileEditVm profileEditVm) {
      if (!_profileService.DoesProfileExist(username)) {
        return NotFound();
      }

      if (!ModelState.IsValid)
        return (View(await _profileService.GetProfileEditVm(username)));

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEdit(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (profileEditVm.ProfileImage != null) {
        var imageCheckResult = profileEditVm.ProfileImage.IsValidImage();
        if (!imageCheckResult.Success) {
          foreach (var error in imageCheckResult.Errors)
            ModelState.AddModelError(string.Empty, error);
          return View(await _profileService.GetProfileEditVm(username));
        }
      }

      var result = await _profileService.UpdateProfile(username, profileEditVm);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(await _profileService.GetProfileEditVm(username));
      }

      return RedirectToAction(nameof(Details));
    }

    [Route("Update/Role/{username}")]
    [AuthorizeRoles(Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(string username, AdminProfileEditVm adminProfileEditVm) {
      if (!_profileService.DoesProfileExist(username)) {
        return NotFound();
      }

        await _profileService.UpdateProfileRole(username, adminProfileEditVm);

      return RedirectToAction(nameof(Details));
    }

    [Route("Update/Block/{username}")]
    [AuthorizeRoles(Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string username, AdminProfileEditVm adminProfileEditVm) {
      if (!_profileService.DoesProfileExist(username)) {
        return NotFound();
      }
      ModelState.AddModelError(string.Empty, "test");
      if (!ModelState.IsValid) {
        RedirectToAction(nameof(Details));
      }

      await _profileService.Block(username, adminProfileEditVm, User);

      return RedirectToAction(nameof(Details));
    }
  }
}
