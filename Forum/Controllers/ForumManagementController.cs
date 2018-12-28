using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [ForumManagementAuthorize]
  [Route("Manage")]
  public class ForumManagementController : Controller {
    private readonly SharedService _sharedService;
    private readonly ForumManagementService _forumManagementService;

    public ForumManagementController(ForumManagementService forumManagementService, SharedService sharedService) {
      _forumManagementService = forumManagementService;
      _sharedService = sharedService;
    }

    [Route("Blocked/Latest")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _forumManagementService.GetForumManagementIndexVmAsync(User));
    }

    [Route("Locked/Topics/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedTopicsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedTopicsVmAsync(User, username));
    }
    [Route("Locked/Topics/All")]
    [HttpGet]
    public async Task<IActionResult> LockedTopicsAll() {
      return View(await _forumManagementService.GetForumManagementLockedTopicsVmAsync(User));
    }

    [Route("Locked/Threads/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedThreadsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedThreadsVmAsync(User, username));
    }

    [Route("Locked/Threads/All")]
    [HttpGet]
    public async Task<IActionResult> LockedThreadsAll() {
      return View(await _forumManagementService.GetForumManagementLockedThreadsVmAsync(User));
    }

    [Route("Locked/Posts/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedPostsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedPostsVmAsync(User, username));
    }

    [Route("Locked/Posts/All")]
    [HttpGet]
    public async Task<IActionResult> LockedPostsAll() {
      return View(await _forumManagementService.GetForumManagementLockedPostsVmAsync(User));
    }

    [Route("Blocked/Members/{username}")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembersUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementBlockedMembersVmAsync(User, username));
    }

    [Route("Blocked/Members/All")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembersAll() {
      return View(await _forumManagementService.GetForumManagementBlockedMembersVmAsync(User));
    }
  }
}