using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ProfileService {
    private readonly ForumDbContext _db;
    private readonly AuthorizationService _authorizationService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
    }

    public async Task<ProfileDetailsVm> GetProfileDetailsVm(string username, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.Include(p => p.PostCreatedByNavigation).Include(p => p.ThreadCreatedByNavigation).FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new ProfileDetailsVm {
        ProfileImage = Convert.ToBase64String(memberFromDb.ProfileImage),
        Username = identityUser.UserName,
        Roles = _userManager.GetRolesAsync(identityUser).Result.ToArray(),
        CreatedOn = memberFromDb.CreatedOn,
        BlockedOn = memberFromDb.BlockedOn,
        BlockedBy = _userManager.FindByIdAsync(memberFromDb.BlockedBy).Result?.UserName,
        BlockedEnd = memberFromDb.BlockedEnd,
        TotalThreads = memberFromDb.ThreadCreatedByNavigation.Count,
        TotalPosts = memberFromDb.PostCreatedByNavigation.Count,
        IsAuthorizedForProfileEdit = _authorizationService.IsAuthorizedForProfileEdit(identityUser.UserName, user)
      };
    }
  }
}
