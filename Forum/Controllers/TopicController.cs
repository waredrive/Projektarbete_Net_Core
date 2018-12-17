using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {

  [Route("")]
  public class TopicController : Controller {

    private readonly TopicService _topicService;

    public TopicController(TopicService topicService) {
      _topicService = topicService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public IActionResult Index() {
      return View();
    }

    [Route("create")]
    [HttpGet]
    public async Task<IActionResult> Create(int id) {

      return View();
    }

    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> Create(TopicCreateVM topicCreateVm) {
      await _topicService.Add(topicCreateVm, User);
      return RedirectToAction(nameof(Index));

    }

    [Route("edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [Route("edit")]
    [HttpPost]
    public async Task<IActionResult> Edit(TopicIndexVM topicIndexVM) {
      return View();
    }

    [Route("delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(TopicIndexVM topicIndexVM) {
      return View();
    }
  }
}