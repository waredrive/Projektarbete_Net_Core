using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModels;
using Forum.Models.ViewModels.ThreadViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Forum/{topicId}")]
  public class ThreadController : Controller {
    private readonly ThreadService _threadService;
    private readonly TopicService _topicService;
    private readonly AuthorizationService _authorizationService;

    public ThreadController(ThreadService threadService, TopicService _topicService, AuthorizationService authorizationService) {
      _threadService = threadService;
      this._topicService = _topicService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int topicId) {
      if (!_topicService.DoesTopicExist(topicId))
        return NotFound();

      return View(await _threadService.GetThreadsIndexVm(topicId, User));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int topicId) {
      if (!_topicService.DoesTopicExist(topicId))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadCreateInTopic(topicId, User))
      return RedirectToAction("AccessDenied", "Account");

      return View(new ThreadCreateVm { TopicId = topicId });
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThreadCreateVm threadCreateVm) {
      if (!ModelState.IsValid)
        return (View(threadCreateVm));

      if (!await _authorizationService.IsAuthorizedForThreadCreateInTopic(threadCreateVm.TopicId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Add(threadCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadEdit(id, User))
      return RedirectToAction("AccessDenied", "Account");

        return View(await _threadService.GetThreadEditVm(id));
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ThreadEditVm threadEditVm) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return (View(threadEditVm));

      if (!await _authorizationService.IsAuthorizedForThreadEdit(threadEditVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Update(threadEditVm, User);
      return RedirectToAction(nameof(Index));

    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadDelete(id, User))
      return RedirectToAction("AccessDenied", "Account");

      return View(await _threadService.GetThreadDeleteVm(id));
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, ThreadDeleteVm threadDeleteVm) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return (View(threadDeleteVm));

      if (!await _authorizationService.IsAuthorizedForThreadDelete(threadDeleteVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Remove(threadDeleteVm);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if(!await _authorizationService.IsAuthorizedForThreadLock(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Unlock));

      return View(await _threadService.GetThreadLockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, ThreadLockVm threadLockVm) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadLockVm);

      if (!await _authorizationService.IsAuthorizedForThreadLock(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_threadService.IsThreadLocked(threadLockVm.ThreadId))
        return RedirectToAction(nameof(Index));

      await _threadService.Lock(threadLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadLock(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Lock));

      return View(await _threadService.GetThreadUnlockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, ThreadUnlockVm threadUnlockVm) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadUnlockVm);

      if (!await _authorizationService.IsAuthorizedForThreadLock(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_threadService.IsThreadLocked(threadUnlockVm.ThreadId))
        return RedirectToAction(nameof(Index));

      await _threadService.Unlock(threadUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}