using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModels;
using Forum.Models.ViewModels.ThreadViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Forum/{topicId}")]
  public class ThreadController : Controller {
    private readonly ThreadService _threadService;
    private readonly AuthorizationService _authorizationService;

    public ThreadController(ThreadService threadService, AuthorizationService authorizationService) {
      _threadService = threadService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int topicId) {
      return View(await _threadService.GetThreadsIndexVm(topicId, User));
    }

    [Route("Create")]
    [HttpGet]
    public IActionResult Create(int topicId) {
      if (_threadService.IsAuthorizedForThreadCreate(topicId, User))
        return View(new ThreadCreateVm { TopicId = topicId });

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThreadCreateVm threadCreateVm) {
      if (!ModelState.IsValid)
        return (View(threadCreateVm));

      if (!_threadService.IsAuthorizedForThreadCreate(threadCreateVm.TopicId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Add(threadCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("UpdateAccount/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if (await _authorizationService.IsAuthorizedForThreadEdit(id, User))
        return View(await _threadService.GetThreadEditVm(id));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("UpdateAccount/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ThreadEditVm threadEditVm) {
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
      if (await _authorizationService.IsAuthorizedForThreadDelete(id, User))
        return View(await _threadService.GetThreadDeleteVm(id));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ThreadDeleteVm threadDeleteVm) {
      if (!ModelState.IsValid)
        return (View(threadDeleteVm));

      if (!await _authorizationService.IsAuthorizedForThreadDelete(threadDeleteVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Remove(threadDeleteVm);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Unlock));

      return View(await _threadService.GetThreadLockVm(id));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(ThreadLockVm threadLockVm) {
      if (!ModelState.IsValid)
        return (View(threadLockVm));

      if (_threadService.IsThreadLocked(threadLockVm.ThreadId))
        return RedirectToAction(nameof(Index));

      await _threadService.Lock(threadLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Lock));

      return View(await _threadService.GetThreadUnlockVm(id));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(ThreadUnlockVm threadUnlockVm) {
      if (!ModelState.IsValid)
        return (View(threadUnlockVm));

      if (!_threadService.IsThreadLocked(threadUnlockVm.ThreadId))
        return RedirectToAction(nameof(Index));

      await _threadService.Unlock(threadUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}