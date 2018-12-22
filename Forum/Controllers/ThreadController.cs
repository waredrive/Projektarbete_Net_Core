using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModel;
using Forum.Models.ViewModels.ThreadViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Forum/{topicId}")]
  public class ThreadController : Controller {
    private readonly ThreadService _threadService;

    public ThreadController(ThreadService threadService) {
      _threadService = threadService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int topicId) {
      return View(await _threadService.GetThreadsIndexVm(topicId, User));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int topicId) {
      if (_threadService.IsTopicLocked(topicId))
        return RedirectToAction("AccessDenied", "Account");

      return View(new ThreadCreateVm {TopicId = topicId});
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThreadCreateVm threadCreateVm) {
      if (!ModelState.IsValid)
        return (View(threadCreateVm));

      if (_threadService.IsTopicLocked(threadCreateVm.TopicId))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Add(threadCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if(await _threadService.IsUserAuthorized(id, User))
      return View(await _threadService.GetThreadEditVm(id));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ThreadEditVm threadEditVm) {
      if (!ModelState.IsValid)
        return (View(threadEditVm));

      if (!await _threadService.IsUserAuthorized(threadEditVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _threadService.Update(threadEditVm, User);
      return RedirectToAction(nameof(Index));

    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ThreadsIndexVm threadsIndexVm) {
      return View();
    }
  }
}