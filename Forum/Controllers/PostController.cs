﻿using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Thread/{threadId}")]
  public class PostController : Controller {
    private readonly PostService _postService;
    private readonly ThreadService _threadService;
    private readonly AuthorizationService _authorizationService;

    public PostController(PostService postService, ThreadService _threadService, AuthorizationService authorizationService) {
      _postService = postService;
      this._threadService = _threadService;
      _authorizationService = authorizationService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int threadId) {
      if (!_threadService.DoesThreadExist(threadId)) {
        return NotFound();
      }

      return View(await _postService.GetPostsIndexVm(threadId, User));
    }


    [Route("Create")]
    [HttpGet]
    public IActionResult Create(int threadId) {
      if (!_threadService.DoesThreadExist(threadId)) {
        return NotFound();
      }

      if (_authorizationService.IsAuthorizedForPostCreate(threadId, User))
        return View(new PostCreateVm { ThreadId = threadId});

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVm postCreateVm) {
      if (!ModelState.IsValid)
        return (View(postCreateVm));

      if (!_authorizationService.IsAuthorizedForPostCreate(postCreateVm.ThreadId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.Add(postCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (await _authorizationService.IsAuthorizedForPostEditAndDelete(id, User))
        return View(await _postService.GetPostEditVm(id));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostEditVm postEditVm) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (!ModelState.IsValid)
        return (View(postEditVm));

      if (!await _authorizationService.IsAuthorizedForPostEditAndDelete(postEditVm.PostId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.Update(postEditVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      if (await _authorizationService.IsAuthorizedForPostEditAndDelete(id, User))
        return View(await _postService.GetPostDeleteVm(id));

      return RedirectToAction("AccessDenied", "Account");
    }

    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, PostDeleteVm postDeleteVm) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (!ModelState.IsValid)
        return (View(postDeleteVm));

      if (!await _authorizationService.IsAuthorizedForPostEditAndDelete(postDeleteVm.PostId, User))
        return RedirectToAction("AccessDenied", "Account");

      await _postService.Remove(postDeleteVm);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (_postService.IsPostLocked(id))
        return RedirectToAction(nameof(Unlock));

      return View(await _postService.GetPostLockVm(id));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, PostLockVm postLockVm) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (!ModelState.IsValid)
        return (View(postLockVm));

      if (_postService.IsPostLocked(postLockVm.PostId))
        return RedirectToAction(nameof(Index));

      await _postService.Lock(postLockVm, User);
      return RedirectToAction(nameof(Index));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (!_postService.IsPostLocked(id))
        return RedirectToAction(nameof(Lock));

      return View(await _postService.GetPostUnlockVm(id));
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, PostUnlockVm postUnlockVm) {
      if (!_postService.DoesPostExist(id)) {
        return NotFound();
      }

      if (!ModelState.IsValid)
        return (View(postUnlockVm));

      if (!_postService.IsPostLocked(postUnlockVm.PostId))
        return RedirectToAction(nameof(Index));

      await _postService.Unlock(postUnlockVm, User);
      return RedirectToAction(nameof(Index));
    }
  }
}