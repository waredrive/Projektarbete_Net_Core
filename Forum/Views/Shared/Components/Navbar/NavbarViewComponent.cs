using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Services;
using Forum.Models.ViewModels.ComponentViewModels.NavbarViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.Navbar {
  public class NavbarViewComponent : ViewComponent {
    private readonly ProfileService _profileService;

    public NavbarViewComponent(ProfileService profileService) {
      _profileService = profileService;
    }

    public async Task<IViewComponentResult> InvokeAsync() {
      return User.Identity.IsAuthenticated ? View(await _profileService.GetNavbarVmAsync(User)) : View(new NavbarVm());
    }
  }
}
