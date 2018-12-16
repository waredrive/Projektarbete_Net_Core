using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.ThreadViewModels;
using Forum.MVC.Models.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers {
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

    [Route("edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [Route("edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(ThreadIndexVM threadIndexVM) {
      return View();
    }

    [Route("delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(ThreadIndexVM threadIndexVM) {
      return View();
    }
  }
}