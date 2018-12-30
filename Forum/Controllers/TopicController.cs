using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Identity;
using Forum.Models.Services;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("")]
  public class TopicController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly TopicService _topicService;

    public TopicController(TopicService topicService, AuthorizationService authorizationService) {
      _topicService = topicService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      ViewBag.ReturnUrl = Request.GetDisplayUrl();
      return View(await _topicService.GetTopicsIndexVmAsync(User));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create() {
      if (!await _authorizationService.IsAuthorizedForCreateTopicAsync(User))
        return RedirectToAction("AccessDenied", "Account");

      return View();
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVm topicCreateVm) {
      if (!ModelState.IsValid)
        return View(topicCreateVm);

      if (!await _authorizationService.IsAuthorizedForCreateTopicAsync(User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.AddAsync(topicCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _topicService.GetTopicCreateVm(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TopicEditVm topicEditVm) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(topicEditVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.UpdateAsync(topicEditVm, User);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _topicService.GetTopicDeleteVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, TopicDeleteVm topicDeleteVm) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(topicDeleteVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.RemoveAsync(topicDeleteVm);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _topicService.IsTopicLocked(id))
        return RedirectToAction(nameof(Unlock));

      return View(await _topicService.GetTopicLockVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, TopicLockVm topicLockVm) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(topicLockVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (await _topicService.IsTopicLocked(topicLockVm.TopicId))
        return RedirectToAction(nameof(Index));

      await _topicService.LockAsync(topicLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!await _topicService.IsTopicLocked(id))
        return RedirectToAction(nameof(Lock));

      return View(await _topicService.GetTopicUnlockVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, TopicUnlockVm topicUnlockVm) {
      if (!await _topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(topicUnlockVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!await _topicService.IsTopicLocked(topicUnlockVm.TopicId))
        return RedirectToAction(nameof(Index));

      await _topicService.UnlockAsync(topicUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}