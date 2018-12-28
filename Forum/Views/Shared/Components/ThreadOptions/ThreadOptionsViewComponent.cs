using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.ThreadOptions
{
    public class ThreadOptionsViewComponent : ViewComponent
    {
      private readonly ThreadService _threadService;

      public ThreadOptionsViewComponent(ThreadService threadService) {
        _threadService = threadService;
      }

      public async Task<IViewComponentResult> InvokeAsync(int threadId) {
        return View(await _threadService.GetThreadOptionsVmAsync(threadId, User));
      }
  }
}
