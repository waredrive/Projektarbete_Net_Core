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

      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (_sharedService.IsDeletedMember(username))
        return RedirectToAction(nameof(ProfileRemoved));

      return View(await _profileService.GetProfileDetailsVmAsync(username, User));
    }

    [Route("Details/ProfileRemoved")]
    [HttpGet]
    public IActionResult ProfileRemoved() {
      return View(_profileService.GetProfileRemovedVmAsync());
    }

    [Route("Update/{username}")]
    [HttpGet]
    public async Task<IActionResult> Edit(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();

      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileEditVmAsync(username));
    }

    [Route("Update/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string username, ProfileEditVm profileEditVm, string returnUrl) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

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

      return RedirectToAction(nameof(Details), new {username = profileEditVm.NewUsername, returnUrl});
    }

    [Route("Role/{username}")]
    [HttpGet]
    public async Task<IActionResult> EditRole(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileChangeRoleAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _profileService.GetProfileRoleEditVmAsync(username));
    }

    [Route("Role/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(string username, ProfileRoleEditVm profileRoleEditVm, string returnUrl) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileRoleEditVm);

      if (!await _authorizationService.IsAuthorizedProfileChangeRoleAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.UpdateProfileRoleAsync(username, profileRoleEditVm);

      return RedirectToAction(nameof(Details), new {username, returnUrl});
    }

    [Route("Block/{username}")]
    [HttpGet]
    public async Task<IActionResult> Block(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();

      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlockedAsync(username))
        return RedirectToAction(nameof(Unblock), new {returnUrl});

      return View(await _profileService.GetProfileBlockVmAsync(username));
    }

    [Route("Block/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(string username, ProfileBlockVm profileBlockVm, string returnUrl) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileBlockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _authorizationService.IsProfileBlockedAsync(username))
        return RedirectToAction(nameof(Details), new {username, returnUrl});

      var result = await _profileService.BlockAsync(username, profileBlockVm, User);

      if (!result.Success) {
        foreach (var error in result.Errors)
          ModelState.AddModelError(string.Empty, error);
        return View(await _profileService.GetProfileBlockVmAsync(username));
      }

      return RedirectToAction(nameof(Details), new {returnUrl});
    }

    [Route("Unblock/{username}")]
    [HttpGet]
    public async Task<IActionResult> Unblock(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");


      if (!await _authorizationService.IsProfileBlockedAsync(username))
        return RedirectToAction(nameof(Unblock), new {returnUrl});

      return View(await _profileService.GetProfileUnblockVmAsync(username));
    }

    [Route("Unblock/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(string username, ProfileUnblockVm profileUnblockVm, string returnUrl) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileUnblockVm);

      if (!await _authorizationService.IsAuthorizedProfileBlockAsync(username, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!await _authorizationService.IsProfileBlockedAsync(username))
        return RedirectToAction(nameof(Details), new {username, returnUrl});

      await _profileService.UnblockAsync(username);

      return RedirectToAction(nameof(Details), new {returnUrl});
    }

    [Route("Delete/{username}")]
    [HttpGet]
    public async Task<IActionResult> Delete(string username, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (await _authorizationService.IsAuthorizedForProfileDeleteAsync(username, User))
        return View(await _profileService.GetProfileDeleteVmAsync(username));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Delete/{username}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string username, ProfileDeleteVm profileDeleteVm, string returnUrl) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      if (!ModelState.IsValid)
        return View(profileDeleteVm);

      if (!await _authorizationService.IsAuthorizedForProfileDeleteAsync(profileDeleteVm.Username, User))
        return RedirectToAction("AccessDenied", "Account");

      await _profileService.RemoveAsync(profileDeleteVm);
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