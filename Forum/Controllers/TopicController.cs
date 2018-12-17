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
    public IActionResult Index() {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("create")]
    [HttpGet]
    public async Task<IActionResult> Create(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVM topicCreateVm) {
      await _topicService.Add(topicCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TopicIndexVM topicIndexVM) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(TopicIndexVM topicIndexVM) {
      return View();
    }
  }
}