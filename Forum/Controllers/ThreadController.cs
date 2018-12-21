using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
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
      return View(await _threadService.GetThreadsIndexVm(topicId));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int topicId) {
      return View(new ThreadCreateVm {TopicId = topicId});
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThreadCreateVm threadCreateVm) {
      await _threadService.Add(threadCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ThreadsIndexVm threadsIndexVm) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(ThreadsIndexVm threadsIndexVm) {
      return View();
    }
  }
}