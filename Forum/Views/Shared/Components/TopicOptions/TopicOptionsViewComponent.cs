using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.TopicOptions {
  public class TopicOptionsViewComponent : ViewComponent {
    private readonly TopicService _topicService;

    public TopicOptionsViewComponent(TopicService topicService) {
      _topicService = topicService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int topicId, string returnUrl = null, string onRemoveReturnUrl = null) {
      var currentUrl = Request.GetDisplayUrl();
      return View(await _topicService.GetTopicOptionsVmAsync(topicId, User, returnUrl ?? currentUrl, onRemoveReturnUrl ?? currentUrl));
    }
  }
}