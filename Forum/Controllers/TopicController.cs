using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("")]
  public class TopicController : Controller {
    private readonly TopicService _topicService;
    private readonly AuthorizationService _authorizationService;

    public TopicController(TopicService topicService, AuthorizationService authorizationService) {
      _topicService = topicService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _topicService.GetTopicsIndexVm(User));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create() {
      if (!await _authorizationService.IsAuthorizedForCreateTopic(User))
      return RedirectToAction("AccessDenied", "Account");

      return View();
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVm topicCreateVm) {
      if (!ModelState.IsValid)
        return (View(topicCreateVm));

      if (!await _authorizationService.IsAuthorizedForCreateTopic(User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.Add(topicCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _topicService.GetTopicCreateVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TopicEditVm topicEditVm) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return (View(topicEditVm));

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.Update(topicEditVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _topicService.GetTopicDeleteVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, TopicDeleteVm topicDeleteVm) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(topicDeleteVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      await _topicService.Remove(topicDeleteVm);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_topicService.IsTopicLocked(id))
        return RedirectToAction(nameof(Unlock));

      return View(await _topicService.GetTopicLockVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, TopicLockVm topicLockVm) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return (View(topicLockVm));

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_topicService.IsTopicLocked(topicLockVm.TopicId))
        return RedirectToAction(nameof(Index));

      await _topicService.Lock(topicLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_topicService.IsTopicLocked(id))
        return RedirectToAction(nameof(Lock));

      return View(await _topicService.GetTopicUnlockVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, TopicUnlockVm topicUnlockVm) {
      if (!_topicService.DoesTopicExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return (View(topicUnlockVm));

      if (!await _authorizationService.IsAuthorizedForTopicEditBlockAndDelete(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_topicService.IsTopicLocked(topicUnlockVm.TopicId))
        return RedirectToAction(nameof(Index));

      await _topicService.Unlock(topicUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}