using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [ForumManagementAuthorize]
  [Route("Manage")]
  public class ForumManagementController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly SharedService _sharedService;
    private readonly ForumManagementService _forumManagementService;

    public ForumManagementController(ForumManagementService forumManagementService,
      AuthorizationService authorizationService, SharedService sharedService) {
      _forumManagementService = forumManagementService;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    [Route("Blocked/Latest")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _forumManagementService.GetForumManagementIndexVm(User));
    }

    [Route("Locked/Topics/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedTopicsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedTopicsVm(User, username));
    }
    [Route("Locked/Topics/All")]
    [HttpGet]
    public async Task<IActionResult> LockedTopicsAll() {
      return View(await _forumManagementService.GetForumManagementLockedTopicsVm(User));
    }

    [Route("Locked/Threads/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedThreadsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedThreadsVm(User, username));
    }

    [Route("Locked/Threads/All")]
    [HttpGet]
    public async Task<IActionResult> LockedThreadsAll() {
      return View(await _forumManagementService.GetForumManagementLockedThreadsVm(User));
    }

    [Route("Locked/Posts/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedPostsUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementLockedPostsVm(User, username));
    }

    [Route("Locked/Posts/All")]
    [HttpGet]
    public async Task<IActionResult> LockedPostsAll() {
      return View(await _forumManagementService.GetForumManagementLockedPostsVm(User));
    }

    [Route("Blocked/Members/{username}")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembersUser(string username) {
      if (!_sharedService.DoesUserAccountExist(username))
        return NotFound();

      return View(await _forumManagementService.GetForumManagementBlockedMembersVm(User, username));
    }

    [Route("Blocked/Members/All")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembersAll() {
      return View(await _forumManagementService.GetForumManagementBlockedMembersVm(User));
    }
  }
}