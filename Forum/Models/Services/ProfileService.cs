using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ProfileService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileService(
      UserManager<IdentityUser> userManager, ForumDbContext db, AuthorizationService authorizationService) {
      _userManager = userManager;
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
        IsAuthorizedForProfileEdit = _authorizationService.IsAuthorizedForProfileEdit(identityUser.UserName, user)
      };
    }

    public async Task<ProfileEditVm> GetAccountEditVm(ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new ProfileEditVm {
        Username = identityUser.UserName,
        ProfileImage = Convert.ToBase64String(memberFromDb.ProfileImage),
        Roles = _userManager.GetRolesAsync(identityUser).Result.Where(r => !r.Equals(Roles.Blocked)).Select(r =>
          new SelectListItem
            {Text = r, Value = r, Selected = r == _userManager.GetRolesAsync(identityUser).Result.First()}).ToArray()
      };
    }

    public async Task<IdentityResult> UpdateProfile(ProfileEditVm profileEditVm, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(profileEditVm.Username);
      var oldUserName = identityUser.UserName;
      var oldRoles = await _userManager.GetRolesAsync(identityUser);
      var result = await _userManager.SetUserNameAsync(identityUser, profileEditVm.Username);

      if (!result.Succeeded)
        return result;

      try {
        var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
        if (user.IsInRole(Roles.Admin)) {
          await _userManager.RemoveFromRolesAsync(identityUser, oldRoles.ToArray());
          await _userManager.AddToRoleAsync(identityUser, profileEditVm.Role);
        }

        if (!string.IsNullOrEmpty(profileEditVm.ProfileImage))
          //TODO: Add Image Save To Db
          //memberFromDb.ProfileImage 

          await _db.SaveChangesAsync();
      }
      catch (Exception) {
        var roles = await _userManager.GetRolesAsync(identityUser);
        await _userManager.RemoveFromRolesAsync(identityUser, roles.ToArray());
        await _userManager.AddToRolesAsync(identityUser, roles);
        await _userManager.SetUserNameAsync(identityUser, oldUserName);
        throw;
      }

      return result;
    }
  }
}