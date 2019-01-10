using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.ThreadOptions {
  public class ThreadOptionsViewComponent : ViewComponent {
    private readonly ThreadService _threadService;

    public ThreadOptionsViewComponent(ThreadService threadService) {
      _threadService = threadService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int threadId, string returnUrl = null,
      string onRemoveReturnUrl = null) {
      var currentUrl = Request.GetDisplayUrl();
      return View(await _threadService.GetThreadOptionsVmAsync(threadId, User, returnUrl ?? currentUrl,
        onRemoveReturnUrl ?? currentUrl));
    }
  }
}