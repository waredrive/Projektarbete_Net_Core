using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.PostOptions
{
    public class PostOptionsViewComponent : ViewComponent
    {
      private readonly PostService _postService;

      public PostOptionsViewComponent(PostService postService) {
        _postService = postService;
      }

    public async Task<IViewComponentResult> InvokeAsync(int postId, string returnUrl = null) {
      return View(await _postService.GetPostOptionsVmAsync(postId, User, returnUrl ?? Request.GetDisplayUrl()));
    }

  }
}
