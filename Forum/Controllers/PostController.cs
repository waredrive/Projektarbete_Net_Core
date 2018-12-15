using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers
{
  [Route("thread/{threadId}")]
  public class PostController : Controller {

    [Route("")]
    [HttpGet]
    public IActionResult Index(int threadId) {
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

    [Route("Edit")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [Route("Edit")]
    [HttpPost]
    public async Task<IActionResult> Edit( PostIndexVM PostIndexVM) {
      return View();
    }

    [Route("Delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("Delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(PostIndexVM PostIndexVM) {
      return View();
    }
  }
}