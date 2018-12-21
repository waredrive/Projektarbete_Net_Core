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

    public TopicController(TopicService topicService) {
      _topicService = topicService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _topicService.GetTopicsIndexVm());
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVm topicCreateVm) {
      await _topicService.Add(topicCreateVm, User);
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
    public async Task<IActionResult> Edit(TopicsIndexVm topicIndexVm) {
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
    public async Task<IActionResult> Delete(TopicsIndexVm topicIndexVm) {
      return View();
    }
  }
}