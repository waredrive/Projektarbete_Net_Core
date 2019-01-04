using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Extensions;
using Forum.Helpers;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [ForumManagementAuthorize]
  [Route("Manage")]
  public class ForumManagementController : Controller {
    private readonly ForumManagementService _forumManagementService;
    private readonly SharedService _sharedService;

    public ForumManagementController(ForumManagementService forumManagementService, SharedService sharedService) {
      _forumManagementService = forumManagementService;
      _sharedService = sharedService;
    }

    [Route("Blocked/Latest")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _forumManagementService.GetForumManagementIndexVmAsync(User));
    }


    [Route("Locked/Topics")]
    [HttpGet]
    public async Task<IActionResult> LockedTopics(int page = 1) {
      return View(await _forumManagementService.GetForumManagementLockedTopicsVmAsync(User, page));
    }

    [Route("Locked/Topics/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedTopics(string username, int page = 1) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(Request.Headers["Referer"].ToString(), "/");

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      return View(await _forumManagementService.GetForumManagementLockedTopicsVmAsync(User, page, username));
    }

    [Route("Locked/Threads")]
    [HttpGet]
    public async Task<IActionResult> LockedThreads(int page = 1) {
      return View(await _forumManagementService.GetForumManagementLockedThreadsVmAsync(User, page));
    }

    [Route("Locked/Threads/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedThreads(string username, int page = 1) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString( Request.Headers["Referer"].ToString(), "/");

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      return View(await _forumManagementService.GetForumManagementLockedThreadsVmAsync(User, page, username));
    }

    [Route("Locked/Posts/")]
    [HttpGet]
    public async Task<IActionResult> LockedPosts(int page = 1) {
      return View(await _forumManagementService.GetForumManagementLockedPostsVmAsync(User, page));
    }

    [Route("Locked/Posts/{username}")]
    [HttpGet]
    public async Task<IActionResult> LockedPosts(string username, int page = 1) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(Request.Headers["Referer"].ToString(), "/");

      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      return View(await _forumManagementService.GetForumManagementLockedPostsVmAsync(User, page, username));
    }

    [Route("Blocked/Members")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembers(int page = 1) {
      return View(await _forumManagementService.GetForumManagementBlockedMembersVmAsync(User, page));
    }

    [Route("Blocked/Members/{username}")]
    [HttpGet]
    public async Task<IActionResult> BlockedMembers(string username, int page = 1) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(Request.Headers["Referer"].ToString(), "/");
      if (!_sharedService.DoesUserAccountExist(username)) {
        TempData.ModalFailed("Profile does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      return View(await _forumManagementService.GetForumManagementBlockedMembersVmAsync(User, page, username));
    }

  }
}