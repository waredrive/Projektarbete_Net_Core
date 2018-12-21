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

    [AuthorizeRoles(Roles.Admin)]
    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVm topicCreateVm) {
      if (!ModelState.IsValid)
        return (View(topicCreateVm));

      await _topicService.Add(topicCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View(await _topicService.GetTopicCreateVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TopicEditVm topicEditVm) {
      if (!ModelState.IsValid)
        return (View(topicEditVm));

      await _topicService.Update(topicEditVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View(await _topicService.GetTopicDeleteVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(TopicDeleteVm topicDeleteVm) {
      if (!ModelState.IsValid)
        return (View(topicDeleteVm));

      await _topicService.Remove(topicDeleteVm);
      return RedirectToAction(nameof(Index));
    }
  }
}