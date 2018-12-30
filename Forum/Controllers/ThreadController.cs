using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Identity;
using Forum.Models.Services;
using Forum.Models.ViewModels.ThreadViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Forum/{topicId}")]
  public class ThreadController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly ThreadService _threadService;
    private readonly TopicService _topicService;

    public ThreadController(ThreadService threadService, TopicService topicService,
      AuthorizationService authorizationService) {
      _threadService = threadService;
      _topicService = topicService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int topicId, int page = 1, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!await _topicService.DoesTopicExist(topicId))
        return NotFound();

      return View(await _threadService.GetThreadsIndexVmAsync(User, topicId, page));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int topicId, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!await _topicService.DoesTopicExist(topicId))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadCreateInTopicAsync(topicId, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(new ThreadCreateVm {TopicId = topicId});
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThreadCreateVm threadCreateVm, string returnUrl) {
      if (!ModelState.IsValid)
        return View(threadCreateVm);

      if (!await _authorizationService.IsAuthorizedForThreadCreateInTopicAsync(threadCreateVm.TopicId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.AddAsync(threadCreateVm, User);
      return Redirect(returnUrl);
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadEditAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _threadService.GetThreadEditVm(id));
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ThreadEditVm threadEditVm, string returnUrl) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadEditVm);

      if (!await _authorizationService.IsAuthorizedForThreadEditAsync(threadEditVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.UpdateAsync(threadEditVm, User);
      return Redirect(returnUrl);
    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _threadService.GetThreadDeleteVm(id));
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, ThreadDeleteVm threadDeleteVm, string returnUrl) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadDeleteVm);

      if (!await _authorizationService.IsAuthorizedForThreadDeleteAsync(threadDeleteVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.RemoveAsync(threadDeleteVm);
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Unlock), new {returnUrl});

      return View(await _threadService.GetThreadLockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, ThreadLockVm threadLockVm, string returnUrl) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadLockVm);

      if (!await _authorizationService.IsAuthorizedForThreadLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_threadService.IsThreadLocked(threadLockVm.ThreadId))
        return Redirect(returnUrl);

      await _threadService.LockAsync(threadLockVm, User);
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!await _authorizationService.IsAuthorizedForThreadLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_threadService.IsThreadLocked(id))
        return RedirectToAction(nameof(Lock), new {returnUrl});

      return View(await _threadService.GetThreadUnlockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, ThreadUnlockVm threadUnlockVm, string returnUrl) {
      if (!_threadService.DoesThreadExist(id))
        return NotFound();

      if (!ModelState.IsValid)
        return View(threadUnlockVm);

      if (!await _authorizationService.IsAuthorizedForThreadLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_threadService.IsThreadLocked(threadUnlockVm.ThreadId))
        return Redirect(returnUrl);

      await _threadService.UnlockAsync(threadUnlockVm, User);
      return Redirect(returnUrl);
    }
  }
}