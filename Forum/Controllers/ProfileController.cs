using System.Threading;
using System.Threading.Tasks;
using Forum.Extensions;
using Forum.Models.Services;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Profile")]
  public class ProfileController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly ProfileService _profileService;
    private readonly SharedService _sharedService;

    public ProfileController(ProfileService profileService, AuthorizationService authorizationService,
      SharedService sharedService) {
      _profileService = profileService;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (_sharedService.IsDeletedMember(username)) {
        TempData.ModalFailed("This Profile has been removed!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      return View(await _profileService.GetProfileDetailsVmAsync(username, User));
    }


    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> Edit(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileEditVmAsync(username));
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, ProfileEditVm profileEditVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(await _profileService.GetProfileEditVmAsync(username));

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (profileEditVm.ProfileImage != null) {
        var imageCheckResult = profileEditVm.ProfileImage.IsValidImage();
        if (!imageCheckResult.Success) {
          foreach (var error in imageCheckResult.Errors)
            ModelState.AddModelError(nameof(profileEditVm.ProfileImage), error);
          return View(await _profileService.GetProfileEditVmAsync(username));
        }
      }

      var result = await _profileService.UpdateProfileAsync(username, profileEditVm);

      if (!result.Succeeded) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error.Description);
        return View(await _profileService.GetProfileEditVmAsync(username));
      }

      TempData.ModalSuccess("The Profile has been updated!");
      return RedirectToAction(nameof(Details), new {username = profileEditVm.NewUsername, returnUrl});
    }

    [Route("Role/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditRole(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedProfileChangeRoleAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileRoleEditVmAsync(username));
    }

    [Route("Role/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(string username, ProfileRoleEditVm profileRoleEditVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(profileRoleEditVm);

      if (!await _authorizationService.IsAuthorizedProfileChangeRoleAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.UpdateProfileRoleAsync(username, profileRoleEditVm);

      TempData.ModalSuccess($"The Role has been updated to {profileRoleEditVm.Role}!");
      return RedirectToAction(nameof(Details), new {username, returnUrl});
    }

    [Route("Block/{username}")]
    [HttpGet]
    public async Task<IActionResult> Block(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlockedAsync(username)) {
        TempData.ModalWarning("The Profile is already blocked!");
        return RedirectToAction(nameof(Unblock), new { returnUrl = ViewBag.ReturnUrl });
      }

      return View(await _profileService.GetProfileBlockVmAsync(username));
    }

    [Route("Block/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string username, ProfileBlockVm profileBlockVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(profileBlockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlockedAsync(username)) {
        TempData.ModalFailed("The Profile is already blocked!");
        return RedirectToAction(nameof(Details), new {username, returnUrl = ViewBag.ReturnUrl});
      }

      var result = await _profileService.BlockAsync(username, profileBlockVm, User);

      if (!result.Success) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error);
        return View(await _profileService.GetProfileBlockVmAsync(username));
      }

      TempData.ModalSuccess("The Profile has been blocked!");
      return RedirectToAction(nameof(Details), new { returnUrl = ViewBag.ReturnUrl });
    }

    [Route("Unblock/{username}")]
    [HttpGet]
    public async Task<IActionResult> Unblock(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");


      if (!await _authorizationService.IsProfileBlockedAsync(username)) {
        TempData.ModalWarning("The Profile is already unblocked!");
        return RedirectToAction(nameof(Block), new { returnUrl = ViewBag.ReturnUrl });
      }

      return View(await _profileService.GetProfileUnblockVmAsync(username));
    }

    [Route("Unblock/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(string username, ProfileUnblockVm profileUnblockVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(profileUnblockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!await _authorizationService.IsProfileBlockedAsync(username)) {
        TempData.ModalFailed("The Profile is already unblocked!");
        return RedirectToAction(nameof(Details), new {username, returnUrl = ViewBag.ReturnUrl});
      }

      await _profileService.UnblockAsync(username);

      TempData.ModalSuccess("The Profile has been unblocked!");
      return RedirectToAction(nameof(Details), new { returnUrl = ViewBag.ReturnUrl });
    }

    [Route("Delete/{username}")]
    [HttpGet]
    public async Task<IActionResult> Delete(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (await _authorizationService.IsAuthorizedForProfileDeleteAsync(username, User))
        return View(await _profileService.GetProfileDeleteVmAsync(username));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Delete/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string username, ProfileDeleteVm profileDeleteVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(profileDeleteVm);

      if (!await _authorizationService.IsAuthorizedForProfileDeleteAsync(profileDeleteVm.Username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.RemoveAsync(profileDeleteVm, User);
      TempData.ModalSuccess("The Profile has been deleted!");
      return Redirect(returnUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("ProfileImage/{username}")]
    public async Task<IActionResult> ProfileImage(string username) {
      return File(await _profileService.GetProfileImage(username), "image/*");
    }

    [HttpGet("Search") ]
    public async Task<JsonResult> Search(string query) {
      var result = _profileService.GetSearchResultJsonAsync(query);
      return Json(await result);
    }
  }
}