using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.ViewModels.NavbarViewModels;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ProfileService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ProfileService(
      UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService) {
      _userManager = userManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
    }

    public async Task<ProfileDetailsVm> GetProfileDetailsVm(string username, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.Include(p => p.PostCreatedByNavigation)
        .Include(p => p.ThreadCreatedByNavigation).FirstOrDefaultAsync(m => m.Id == identityUser.Id);

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
        IsAuthorizedForProfileEdit = await _authorizationService.IsAuthorizedForAccountEdit(identityUser.UserName, user)
      };
    }

    public async Task<ProfileEditVm> GetProfileEditVm(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new ProfileEditVm {
        OldUsername = identityUser.UserName,
        NewUsername = identityUser.UserName,
        Roles = _roleManager.Roles.Select(r =>
          new SelectListItem { Text = r.Name, Value = r.Name, Selected = r.Name == _userManager.GetRolesAsync(identityUser).Result.First() }).ToArray()
      };
    }

    public async Task<IdentityResult> UpdateProfile(string username, ProfileEditVm profileEditVm, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var oldUserName = identityUser.UserName;
      var oldRoles = await _userManager.GetRolesAsync(identityUser);
      var result = await _userManager.SetUserNameAsync(identityUser, profileEditVm.NewUsername);

      if (!result.Succeeded)
        return result;

      try {
        var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
        if (user.IsInRole(Roles.Admin) && !string.IsNullOrEmpty(profileEditVm.Role)) {
          await _userManager.RemoveFromRolesAsync(identityUser, oldRoles.ToArray());
          await _userManager.AddToRoleAsync(identityUser, profileEditVm.Role);
        }

        if (profileEditVm.ProfileImage != null) {
          using (var memoryStream = new MemoryStream()) {
            await profileEditVm.ProfileImage.CopyToAsync(memoryStream);
            memberFromDb.ProfileImage = memoryStream.ToArray();
          }
        }

        await _db.SaveChangesAsync();
      } catch (Exception) {
        var roles = await _userManager.GetRolesAsync(identityUser);
        await _userManager.RemoveFromRolesAsync(identityUser, roles.ToArray());
        await _userManager.AddToRolesAsync(identityUser, roles);
        await _userManager.SetUserNameAsync(identityUser, oldUserName);
        throw;
      }

      return result;
    }

    public async Task<NavbarVm> GetNavbarVm(IPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      return new NavbarVm {ProfileImage = Convert.ToBase64String(memberFromDb.ProfileImage)};
    }

  public bool DoesProfileExist(string username) {
    return _userManager.FindByNameAsync(username).Result != null;
  }
}
}