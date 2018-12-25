using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Services;
using Forum.Models.ViewModels.ComponentViewModels.AdminProfileEditViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.AdminProfileEdit
{
    public class AdminProfileEditViewComponent : ViewComponent
    {
      private readonly ProfileService _profileService;

      public AdminProfileEditViewComponent(ProfileService profileService) {
        _profileService = profileService;
      }

      public async Task<IViewComponentResult> InvokeAsync(string username) {
        return User.IsInRole(Roles.Admin) ? View(await _profileService.GetAdminProfileEditVm(username)) : View(new AdminProfileEditVm());
      }
  }
}