﻿using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Extensions;
using Forum.Helpers;
using Forum.Models.Identity;
using Forum.Models.Services;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("")]
  public class TopicController : Controller {
    private readonly AuthorizationService _authorizationService;
    private readonly SharedService _sharedService;
    private readonly TopicService _topicService;

    public TopicController(TopicService topicService, AuthorizationService authorizationService,
      SharedService sharedService) {
      _topicService = topicService;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index() {
      ViewBag.ReturnUrl = Request.GetDisplayUrl();
      return View(await _topicService.GetTopicsIndexVmAsync(User));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(string returnUrl = null) {
      ViewBag.ReturnUrl = returnUrl ?? Request.Headers["Referer"].ToString();
      if (!await _authorizationService.IsAuthorizedForCreateTopicAsync(User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View();
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicCreateVm topicCreateVm, string returnUrl) {
      ViewBag.ReturnUrl = returnUrl;
      if (!ModelState.IsValid)
        return View(topicCreateVm);

      if (!await _authorizationService.IsAuthorizedForCreateTopicAsync(User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _topicService.AddAsync(topicCreateVm, User);

      TempData.ModalSuccess("The Topic has been created!");
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View(await _topicService.GetTopicCreateVm(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Update/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TopicEditVm topicEditVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      ;
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(topicEditVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _topicService.UpdateAsync(topicEditVm, User);

      TempData.ModalSuccess("The Topic has been updated!");
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null, string onRemoveReturnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      ViewBag.OnRemoveReturnUrl =
        StringHelper.FirstValidString(onRemoveReturnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      return View(await _topicService.GetTopicDeleteVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Delete/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, TopicDeleteVm topicDeleteVm, string returnUrl,
      string onRemoveReturnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      ViewBag.OnRemoveReturnUrl = StringHelper.FirstValidString(onRemoveReturnUrl, "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(topicDeleteVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      await _topicService.RemoveAsync(topicDeleteVm);

      TempData.ModalSuccess("The Topic has been deleted!");
      return Redirect(ViewBag.OnRemoveReturnUrl);
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Lock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (await _topicService.IsTopicLocked(id)) {
        TempData.ModalWarning("The Topic is already locked!");
        return RedirectToAction(nameof(Unlock), new {returnUrl = ViewBag.ReturnUrl});
      }

      return View(await _topicService.GetTopicLockVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Lock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id, TopicLockVm topicLockVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(topicLockVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (await _topicService.IsTopicLocked(topicLockVm.TopicId)) {
        TempData.ModalFailed("The Topic is already locked!");
        return Redirect(returnUrl);
      }

      await _topicService.LockAsync(topicLockVm, User);
      TempData.ModalSuccess("The Topic has been locked!");
      return Redirect(returnUrl);
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpGet]
    public async Task<IActionResult> Unlock(int id, string returnUrl = null) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, Request.Headers["Referer"].ToString(), "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (!await _topicService.IsTopicLocked(id)) {
        TempData.ModalWarning("The Topic is already unlocked!");
        return RedirectToAction(nameof(Lock), new {returnUrl = ViewBag.ReturnUrl});
      }

      return View(await _topicService.GetTopicUnlockVmAsync(id));
    }

    [RolesAuthorize(Roles.Admin)]
    [Route("Unlock/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(int id, TopicUnlockVm topicUnlockVm, string returnUrl) {
      ViewBag.ReturnUrl = StringHelper.FirstValidString(returnUrl, "/");
      if (!await _sharedService.DoesTopicExist(id)) {
        TempData.ModalFailed("Topic does not exist!");
        return Redirect(ViewBag.ReturnUrl);
      }

      if (!ModelState.IsValid)
        return View(topicUnlockVm);

      if (!await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(id, User))
        return this.RedirectToControllerAction<AccountController>(nameof(AccountController.AccessDenied));

      if (!await _topicService.IsTopicLocked(topicUnlockVm.TopicId)) {
        TempData.ModalFailed("The Topic is already unlocked");
        return Redirect(returnUrl);
      }

      await _topicService.UnlockAsync(topicUnlockVm);
      TempData.ModalSuccess("The Topic has been unlocked!");
      return Redirect(returnUrl);
    }
  }
}