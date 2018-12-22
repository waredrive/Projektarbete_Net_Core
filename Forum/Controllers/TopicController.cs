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

    [AuthorizeRoles(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (_topicService.IsTopicLocked(id))
        RedirectToAction(nameof(Unlock));

      return View(await _topicService.GetTopicLockVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(TopicLockVm topicLockVm) {
      if (!ModelState.IsValid)
        return (View(topicLockVm));

      if (_topicService.IsTopicLocked(topicLockVm.TopicId))
        RedirectToAction(nameof(Index));

      await _topicService.Lock(topicLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!_topicService.IsTopicLocked(id))
        RedirectToAction(nameof(Lock));

      return View(await _topicService.GetTopicLockVm(id));
    }

    [AuthorizeRoles(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(TopicUnlockVm topicUnlockVm) {
      if (!ModelState.IsValid)
        return (View(topicUnlockVm));

      if (!_topicService.IsTopicLocked(topicUnlockVm.TopicId))
        RedirectToAction(nameof(Index));

      await _topicService.Unlock(topicUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}