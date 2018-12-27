using System.Threading.Tasks;
using Forum.Extensions;
using Forum.Models.Services;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Profile")]
  public class ProfileController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService, AuthorizationService authorizationService) {
      _profileService = profileService;
      _authorizationService = authorizationService;
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (await _authorizationService.IsProfileInternal(username))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileDetailsVm(username, User));
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> Edit(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEdit(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileEditVm(username));
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, ProfileEditVm profileEditVm) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(await _profileService.GetProfileEditVm(username));

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEdit(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (profileEditVm.ProfileImage != null) {
        var imageCheckResult = profileEditVm.ProfileImage.IsValidImage();
        if (!imageCheckResult.Success) {
          foreach (var error in imageCheckResult.Errors)
            ModelState.AddModelError(nameof(profileEditVm.ProfileImage), error);
          return View(await _profileService.GetProfileEditVm(username));
        }
      }

      var result = await _profileService.UpdateProfile(username, profileEditVm);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(await _profileService.GetProfileEditVm(username));
      }

      return RedirectToAction(nameof(Details), new {username = profileEditVm.NewUsername});
    }

    [Route("Role/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditRole(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileChangeRole(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileRoleEditVm(username));
    }

    [Route("Role/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(string username, ProfileRoleEditVm profileRoleEditVm) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileRoleEditVm);

      if (!await _authorizationService.IsAuthorizedProfileChangeRole(username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.UpdateProfileRole(username, profileRoleEditVm);

      return RedirectToAction(nameof(Details), new {username});
    }

    [Route("Block/{username}")]
    [HttpGet]
    public async Task<IActionResult> Block(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileBlock(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlocked(username))
        return RedirectToAction(nameof(Unblock));

      return View(await _profileService.GetProfileBlockVm(username));
    }

    [Route("Block/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string username, ProfileBlockVm profileBlockVm) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileBlockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlock(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlocked(username))
        return RedirectToAction(nameof(Details), new {username});

      var result = await _profileService.Block(username, profileBlockVm, User);

      if (!result.Success) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error);
        return View(await _profileService.GetProfileBlockVm(username));
      }

      return RedirectToAction(nameof(Details));
    }

    [Route("Unblock/{username}")]
    [HttpGet]
    public async Task<IActionResult> Unblock(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileBlock(username, User))
        return RedirectToAction("AccessDenied", "Account");


      if (!await _authorizationService.IsProfileBlocked(username))
        return RedirectToAction(nameof(Unblock));

      return View(await _profileService.GetProfileUnblockVm(username));
    }

    [Route("Unblock/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(string username, ProfileUnblockVm profileUnblockVm) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileUnblockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlock(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!await _authorizationService.IsProfileBlocked(username))
        return RedirectToAction(nameof(Details), new {username});

      await _profileService.Unblock(username, profileUnblockVm, User);

      return RedirectToAction(nameof(Details));
    }

    [Route("Delete/{username}")]
    [HttpGet]
    public async Task<IActionResult> Delete(string username) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (await _authorizationService.IsAuthorizedForProfileDelete(username, User))
        return View(await _profileService.GetProfileDeleteVm(username));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Delete/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string username, ProfileDeleteVm profileDeleteVm) {
      if (!_profileService.DoesProfileExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileDeleteVm);

      if (!await _authorizationService.IsAuthorizedForProfileDelete(profileDeleteVm.Username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.Remove(profileDeleteVm);
      return RedirectToAction("Index", "Topic");
    }
  }
}