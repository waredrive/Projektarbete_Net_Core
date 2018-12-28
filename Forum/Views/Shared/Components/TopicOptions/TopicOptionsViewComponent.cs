using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.TopicManageOptions {
  public class TopicOptionsViewComponent : ViewComponent {
    private readonly TopicService _topicService;

    public TopicOptionsViewComponent(TopicService topicService) {
      _topicService = topicService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int topicId) {
      return View(await _topicService.GetTopicOptionsVmAsync(topicId, User));
    }
  }
}