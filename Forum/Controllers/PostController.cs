using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.PostViewModel;
using Forum.Models.ViewModels.PostViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Thread/{threadId}")]
  public class PostController : Controller {
    private readonly PostService _postService;

    public PostController(PostService postService) {
      _postService = postService;
    }

    [AllowAnonymous]
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Index(int threadId) {
      return View(await _postService.GetTopicsIndexVm(threadId));
    }


    [Route("Create")]
    [HttpGet]
    public async Task<IActionResult> Create(int threadId) {
      return View(new PostCreateVm { ThreadId = threadId });
    }

    [Route("Create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostCreateVm postCreateVm) {
      await _postService.Add(postCreateVm, User);
      return RedirectToAction(nameof(Index));
    }

    [Route("Edit/{id}")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id) {
      return View();
    }

    [Route("Edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PostsIndexVm postsIndexVm) {
      return View();
    }

    [Route("Delete/{id}")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id) {
      return View();
    }

    [Route("Delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(PostsIndexVm postsIndexVm) {
      return View();
    }
  }
}