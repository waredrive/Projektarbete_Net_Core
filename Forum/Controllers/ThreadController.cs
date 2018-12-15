using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers {
  [Route("forum/{topicId}")]
  public class ThreadController : Controller {

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

    [Route("edit/{id}")]
    [HttpPost]
    public async Task<IActionResult> Edit(ThreadIndexVM threadIndexVM) {
      return View();
    }

    [Route("delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("delete/{id}")]
    [HttpPost]
    public async Task<IActionResult> Delete(ThreadIndexVM threadIndexVM) {
      return View();
    }
  }
}