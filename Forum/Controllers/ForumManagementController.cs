using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [ForumManagementAuthorize]
  [Route("Manage")]
  public class ForumManagementController : Controller {
    private readonly ForumManagementService _forumManagementService;
    private readonly AuthorizationService _authorizationService;

    public ForumManagementController(ForumManagementService forumManagementService, AuthorizationService authorizationService) {
      _forumManagementService = forumManagementService;
      _authorizationService = authorizationService;
    }

    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      return View(await _forumManagementService.GetForumManagementIndexVm(User));
    }

    [Route("Blocked/Topics")]
    [HttpGet]
    public IActionResult BlockedTopics() {
      return RedirectToAction("Details", "Profile", new { username = User.Identity.Name });
    }

    [Route("Blocked/Threads")]
    [HttpGet]
    public IActionResult BlockedThreads() {
      return RedirectToAction("Details", "Profile", new { username = User.Identity.Name });
    }

    [Route("Users")]
    [HttpGet]
    public IActionResult Users() {
      return RedirectToAction("Details", "Profile", new { username = User.Identity.Name });
    }

    [Route("Blocked/Users")]
    [HttpGet]
    public IActionResult BlockedUsers() {
      return RedirectToAction("Details", "Profile", new { username = User.Identity.Name });
    }

    [Route("Blocked/Posts")]
    [HttpGet]
    public IActionResult BlockedPosts() {
      return RedirectToAction("Details", "Profile", new { username = User.Identity.Name });
    }
  }
}