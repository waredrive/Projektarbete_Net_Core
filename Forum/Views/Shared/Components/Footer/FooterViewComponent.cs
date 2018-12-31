using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.Footer {
  public class FooterViewComponent : ViewComponent {
    private readonly SharedService _sharedService;

    public FooterViewComponent(SharedService sharedService) {
      _sharedService = sharedService;
    }

    public async Task<IViewComponentResult> InvokeAsync() {
      return View(await _sharedService.GetFooterVmAsync());
    }

  }
}
