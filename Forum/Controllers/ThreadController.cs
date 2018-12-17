using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.ThreadViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("forum/{topicId}")]
  public class ThreadController : Controller {

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public IActionResult Index(int topicId) {
      return View();
    }

    [Route("create")]
    [HttpGet]
    public async Task<IActionResult> Create(int id) {
      return View();
    }

    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> Create(TopicIndexVM topicIndexVM) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(ThreadIndexVM threadIndexVM) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [AuthorizeRoles(Roles.Admin, Roles.Moderator)]
    [Route("delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(ThreadIndexVM threadIndexVM) {
      return View();
    }
  }
}