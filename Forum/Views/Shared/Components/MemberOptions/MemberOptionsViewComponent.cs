﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Views.Shared.Components.MemberOptions
{
    public class MemberOptionsViewComponent : ViewComponent
    {
      private readonly ProfileService _profileService;

      public MemberOptionsViewComponent(ProfileService profileService) {
        _profileService = profileService;
      }

    public async Task<IViewComponentResult> InvokeAsync(string username) {
      return View(await _profileService.GetMemberOptionsVmAsync(username, User));
    }
  }
}
