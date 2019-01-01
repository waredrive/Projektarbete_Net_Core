using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Extensions;
using Forum.Models.Identity;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Thread/{threadId}")]
  public class PostController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly PostService _postService;
    private readonly ThreadService _threadService;

    public PostController(PostService postService, ThreadService threadService,
      AuthorizationService authorizationService) {
      _postService = postService;
      _threadService = threadService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int threadId, int? postId = null, int page = 1) {
      if (!_threadService.DoesThreadExist(threadId))
        return NotFound();

      return View(await _postService.GetPostsIndexVmAsync(User, threadId, page, postId));
    }

    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int threadId, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_threadService.DoesThreadExist(threadId)) {
        TempData.ModalFailed("Thread does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostCreateInThreadAsync(threadId, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(new PostCreateVm {ThreadId = threadId});
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVm postCreateVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;

      if (!ModelState.IsValid)
        return View(postCreateVm);

      if (!_threadService.DoesThreadExist(postCreateVm.ThreadId)) {
        TempData.ModalFailed("Thread does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostCreateInThreadAsync(postCreateVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.AddAsync(postCreateVm, User);

      TempData.ModalSuccess("The Post has been created!");
      return Redirect(returnUrl);
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _postService.GetPostEditVm(id));
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostEditVm postEditVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postEditVm);

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(postEditVm.PostId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.UpdateAsync(postEditVm, User);
      TempData.ModalSuccess("The Post has been updated!");
      return Redirect(returnUrl);
    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      return View(await _postService.GetPostDeleteVm(id));
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, PostDeleteVm postDeleteVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postDeleteVm);

      if (!await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(postDeleteVm.PostId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.RemoveAsync(postDeleteVm);
      TempData.ModalSuccess("The Post has been deleted!");
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_postService.IsPostLocked(id)) {
        TempData.ModalWarning("The Post is already locked!");
        return RedirectToAction(nameof(Unlock), new { returnUrl = ViewBag.ReturnUrl });
      }

      return View(await _postService.GetPostLockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, PostLockVm postLockVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postLockVm);

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (_postService.IsPostLocked(postLockVm.PostId)) {
        TempData.ModalFailed("The Post is already locked!");
        return Redirect(returnUrl);
      }

      await _postService.LockAsync(postLockVm, User);
      TempData.ModalSuccess("The Post has been Locked!");
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User)) 
        return RedirectToAction("AccessDenied", "Account");

      if (!_postService.IsPostLocked(id)) {
        TempData.ModalWarning("The Post is already unlocked!");
        return RedirectToAction(nameof(Lock), new { returnUrl = ViewBag.ReturnUrl });
      }

      return View(await _postService.GetPostUnlockVm(id));
    }

    [RolesAuthorize(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, PostUnlockVm postUnlockVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!_postService.DoesPostExist(id)) {
        TempData.ModalFailed("Post does not exist!");
        return Redirect(string.IsNullOrEmpty(ViewBag.ReturnUrl) ? "/" : ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(postUnlockVm);

      if (!await _authorizationService.IsAuthorizedForPostLockAsync(id, User))
        return RedirectToAction("AccessDenied", "Account");

      if (!_postService.IsPostLocked(postUnlockVm.PostId)) {
        TempData.ModalFailed("The Post is already unlocked!");
        return Redirect(returnUrl);
      }

      await _postService.UnlockAsync(postUnlockVm, User);
      TempData.ModalSuccess("The Post has been unlocked!");
      return Redirect(returnUrl);
    }
  }
}