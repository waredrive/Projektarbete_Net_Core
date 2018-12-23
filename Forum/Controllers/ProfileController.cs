using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers {
  [Route("Profile")]
  public class ProfileController : Controller {
    private readonly ProfileService _profileService;
    private readonly AuthorizationService _authorizationService;

    public ProfileController(ProfileService profileService, AuthorizationService authorizationService) {
      _profileService = profileService;
      _authorizationService = authorizationService;
    }

    [Route("Details/{username}")]
    [HttpGet]
    public async Task<IActionResult> Details(string username) {
        return View(await _profileService.GetProfileDetailsVm(username, User));
    }
  }
}
