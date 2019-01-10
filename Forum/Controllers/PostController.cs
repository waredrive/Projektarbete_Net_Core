using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Extensions;
using Forum.Helpers;
using Forum.Models.Identity;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Forum/{topicId}/{threadId}")]
  public class PostController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly PostService _postService;
    private readonly SharedService _sharedService;

    public PostController(PostService postService, SharedService sharedService,
      AuthorizationService authorizationService) {
      _postService = postService;
      _sharedService = sharedService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int topicId, int threadId, int? postId = null, int page = 1) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(Request.Headers["Referer"].ToString(), "/");

      if (!await _sharedService.DoesTopicExist(topicId)) {
        TempData.ModalFailed("Topic does not exist!");
        return this.RedirectToControllerAction<TopicController>(nameof(TopicController.Index));
      }

      if (!await _sharedService.DoesThreadExist(threadId)) {
        TempData.ModalFailed("Thread does not exist!");
        return this.RedirectToControllerAction<ThreadController>(nameof(ThreadController.Index), new {threadId});
      }

      return View(await _postService.GetPostsIndexVmAsync(User, threadId, page, postId));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int topicId, int threadId, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!await _sharedService.DoesTopicExist(topicId)) {
        TempData.ModalFailed("The topic you trying to post to does not exist!");
        return this.RedirectToControllerAction<TopicController>(nameof(TopicController.Index));
      }

      if (!await _sharedService.DoesThreadExist(threadId)) {
        TempData.ModalFailed("Thread you trying to post to does not exist!");
        return this.RedirectToControllerAction<ThreadController>(nameof(ThreadController.Index), new {threadId});
      }

      if (!await _authorizationService.IsAuthorizedForPostCreateInThreadAsync(threadId, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View(new PostCreateVm {ThreadId = threadId});
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int topicId, int threadId, PostCreateVm postCreateVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");

      if (!await _sharedService.DoesTopicExist(topicId)) {
        TempData.ModalFailed("The topic you trying to post to does not exist!");
        return this.RedirectToControllerAction<TopicController>(nameof(TopicController.Index));
      }

      if (!await _sharedService.DoesThreadExist(threadId)) {
        TempData.ModalFailed("Thread you trying to post to does not exist!");
        return this.RedirectToControllerAction<ThreadController>(nameof(ThreadController.Index), new {threadId});
      }

      if (!ModelState.IsValid)
        return View(postCreateVm);

      if (!await _authorizationService.IsAuthorizedForPostCreateInThreadAsync(postCreateVm.ThreadId, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _postService.AddAsync(postCreateVm, User);

      TempData.ModalSuccess("The Post has been created!");
      return Redirect(ViewBag.ReturnUrl);
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View(await _postService.GetPostEditVm(id));
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostEditVm postEditVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postEditVm);

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(postEditVm.PostId, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _postService.UpdateAsync(postEditVm, User);
      TempData.ModalSuccess("The Post has been updated!");
      return Redirect(ViewBag.ReturnUrl);
    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View(await _postService.GetPostDeleteVm(id));
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, PostDeleteVm postDeleteVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postDeleteVm);

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(postDeleteVm.PostId, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _postService.RemoveAsync(postDeleteVm);
      TempData.ModalSuccess("The Post has been deleted!");
      return Redirect(ViewBag.ReturnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (_postService.IsPostLocked(id)) {
        TempData.ModalWarning("The Post is already locked!");
        return RedirectToAction(nameof(Unlock), new {returnUrl = ViewBag.ReturnUrl});
      }

      return View(await _postService.GetPostLockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, PostLockVm postLockVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postLockVm);

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (_postService.IsPostLocked(postLockVm.PostId)) {
        TempData.ModalFailed("The Post is already locked!");
        return Redirect(ViewBag.ReturnUrl);
      }

      await _postService.LockAsync(postLockVm, User);
      TempData.ModalSuccess("The Post has been Locked!");
      return Redirect(ViewBag.ReturnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (!_postService.IsPostLocked(id)) {
        TempData.ModalWarning("The Post is already unlocked!");
        return RedirectToAction(nameof(Lock), new {returnUrl = ViewBag.ReturnUrl});
      }

      return View(await _postService.GetPostUnlockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, PostUnlockVm postUnlockVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postUnlockVm);

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (!_postService.IsPostLocked(postUnlockVm.PostId)) {
        TempData.ModalFailed("The Post is already unlocked!");
        return Redirect(ViewBag.ReturnUrl);
      }

      await _postService.UnlockAsync(postUnlockVm, User);
      TempData.ModalSuccess("The Post has been unlocked!");
      return Redirect(ViewBag.ReturnUrl);
    }
  }
}