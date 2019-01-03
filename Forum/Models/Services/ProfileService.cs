using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Identity;
using Forum.Models.ViewModels.ComponentViewModels.MemberOptionsViewModels;
using Forum.Models.ViewModels.ComponentViewModels.NavbarViewModels;
using Forum.Models.ViewModels.ProfileViewModels;
using Forum.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ProfileService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SharedService _sharedService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileService(
      UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
      SignInManager<IdentityUser> signInManager, ForumDbContext db, AuthorizationService authorizationService,
      SharedService sharedService) {
      _userManager = userManager;
      _roleManager = roleManager;
      _signInManager = signInManager;
      _db = db;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    public async Task<ProfileDetailsVm> GetProfileDetailsVmAsync(string username, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.Include(m => m.PostCreatedByNavigation)
        .Include(m => m.ThreadCreatedByNavigation).FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      var blockedBy = await _userManager.FindByIdAsync(memberFromDb.BlockedBy);
      var roles = await _userManager.GetRolesAsync(identityUser);

      return new ProfileDetailsVm {
        Username = identityUser.UserName,
        Roles = roles.ToArray(),
        CreatedOn = memberFromDb.CreatedOn,
        BlockedOn = memberFromDb.BlockedOn,
        BlockedBy = blockedBy?.UserName,
        BlockedEnd = memberFromDb.BlockedEnd,
        TotalThreads = memberFromDb.ThreadCreatedByNavigation.Count,
        TotalPosts = memberFromDb.PostCreatedByNavigation.Count,
        IsAuthorizedForAccountView =
          await _authorizationService.IsAuthorizedForAccountDetailsViewAsync(identityUser.UserName, user),
        IsUserOwner = IsUserOwner(identityUser.UserName, user)
      };
    }

    public async Task<ProfileEditVm> GetProfileEditVmAsync(string oldUsername) {
      var identityUser = await _userManager.FindByNameAsync(oldUsername);

      return new ProfileEditVm {
        Username = identityUser.UserName,
        NewUsername = identityUser.UserName
      };
    }

    public async Task<IdentityResult> UpdateProfileAsync(string username, ProfileEditVm profileEditVm) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var oldUserName = identityUser.UserName;
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      if (_sharedService.IsDeletedMember(identityUser.UserName))
        return IdentityResult.Failed(new IdentityError { Description = "This member cannot be edited" });

      var result = await _userManager.SetUserNameAsync(identityUser, profileEditVm.NewUsername);

      if (!result.Succeeded)
        return result;

      try {
        if (profileEditVm.ProfileImage != null)
          using (var memoryStream = new MemoryStream()) {
            await profileEditVm.ProfileImage.CopyToAsync(memoryStream);
            memberFromDb.ProfileImage = memoryStream.ToArray();
          }

        await _db.SaveChangesAsync();

        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(identityUser, false, identityUser.Id);
      } catch (Exception) {
        await _userManager.SetUserNameAsync(identityUser, oldUserName);
        throw;
      }

      return result;
    }

    public async Task<ProfileRoleEditVm> GetProfileRoleEditVmAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var role = (await _userManager.GetRolesAsync(identityUser)).First();

      return new ProfileRoleEditVm {
        Role = role,
        Username = identityUser.UserName,
        Roles = _roleManager.Roles.Select(r =>
          new SelectListItem {
            Text = r.Name,
            Value = r.Name,
            Selected = r.Name == role
          }).ToArray()
      };
    }

    public async Task<ProfileBlockVm> GetProfileBlockVmAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);

      return new ProfileBlockVm {
        Username = identityUser.UserName,
        BlockedEnd = DateTime.UtcNow
      };
    }

    public async Task<CustomValidationResult> BlockAsync(string username, ProfileBlockVm profileBlockVm,
      ClaimsPrincipal user) {
      var validationResult = new CustomValidationResult();

      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      if (_sharedService.IsDeletedMember(identityUser.UserName)) {
        validationResult.Errors.Add("This profile cannot be blocked.");
        return validationResult;
      }

      if (profileBlockVm.BlockedEnd == null) {
        validationResult.Errors.Add("A date must be set.");
        return validationResult;
      }

      if (profileBlockVm.BlockedEnd < DateTime.UtcNow) {
        validationResult.Errors.Add("A date must be set in the future.");
        return validationResult;
      }

      memberFromDb.BlockedEnd = profileBlockVm.BlockedEnd;
      memberFromDb.BlockedOn = DateTime.UtcNow;
      memberFromDb.BlockedBy = (await _userManager.FindByNameAsync(user.Identity.Name)).Id;

      await _db.SaveChangesAsync();
      return validationResult;
    }

    public async Task<ProfileUnblockVm> GetProfileUnblockVmAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.Include(p => p.PostCreatedByNavigation)
        .Include(p => p.ThreadCreatedByNavigation).FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      var blockedBy = await _userManager.FindByIdAsync(memberFromDb.BlockedBy);

      return new ProfileUnblockVm {
        Username = identityUser.UserName,
        BlockedEnd = memberFromDb.BlockedEnd,
        BlockedOn = memberFromDb.BlockedOn,
        BlockedBy = blockedBy?.UserName
      };
    }

    public async Task UnblockAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      memberFromDb.BlockedEnd = null;
      memberFromDb.BlockedOn = null;
      memberFromDb.BlockedBy = null;

      await _db.SaveChangesAsync();
    }

    public async Task UpdateProfileRoleAsync(string username, ProfileRoleEditVm profileRoleEditVm) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var oldRoles = await _userManager.GetRolesAsync(identityUser);

      try {
        if (!string.IsNullOrEmpty(profileRoleEditVm.Role) && !_sharedService.IsDeletedMember(identityUser.UserName)) {
          await _userManager.RemoveFromRolesAsync(identityUser, oldRoles.ToArray());
          await _userManager.AddToRoleAsync(identityUser, profileRoleEditVm.Role);
          await _db.SaveChangesAsync();
        }
      } catch (Exception) {
        var roles = await _userManager.GetRolesAsync(identityUser);
        await _userManager.RemoveFromRolesAsync(identityUser, roles.ToArray());
        await _userManager.AddToRolesAsync(identityUser, roles);
        await _db.SaveChangesAsync();
        throw;
      }
    }

    public async Task<ProfileDeleteVm> GetProfileDeleteVmAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.Include(p => p.PostCreatedByNavigation)
        .Include(p => p.ThreadCreatedByNavigation).FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      var blockedBy = await _userManager.FindByIdAsync(memberFromDb.BlockedBy);
      var roles = await _userManager.GetRolesAsync(identityUser);

      return new ProfileDeleteVm {
        ProfileImage = Convert.ToBase64String(memberFromDb.ProfileImage),
        Username = identityUser.UserName,
        Roles = roles.ToArray(),
        CreatedOn = memberFromDb.CreatedOn,
        BlockedOn = memberFromDb.BlockedOn,
        BlockedBy = blockedBy?.UserName,
        BlockedEnd = memberFromDb.BlockedEnd,
        TotalThreads = memberFromDb.ThreadCreatedByNavigation.Count,
        TotalPosts = memberFromDb.PostCreatedByNavigation.Count
      };
    }

    public async Task RemoveAsync(ProfileDeleteVm profileDeleteVm, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(profileDeleteVm.Username);
      if (_sharedService.IsDeletedMember(identityUser.UserName))
        return;

      var userIsOwner = IsUserOwner(profileDeleteVm.Username, user);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      memberFromDb.BlockedBy = null;
      memberFromDb.BirthDate = DeletedMember.BirthDate;
      memberFromDb.BlockedOn = null;
      memberFromDb.BlockedEnd = null;
      memberFromDb.FirstName = DeletedMember.FirstName;
      memberFromDb.LastName = DeletedMember.LastName;
      memberFromDb.ProfileImage = await _sharedService.GetDefaultProfileImage();

      var roles = await _userManager.GetRolesAsync(identityUser);
      await _userManager.RemoveFromRolesAsync(identityUser, roles.ToArray());
      await _userManager.AddToRoleAsync(identityUser, Roles.User);
      var userCount = _userManager.Users.Count(i => _sharedService.IsDeletedMember(i.UserName));
      await _userManager.SetUserNameAsync(identityUser, DeletedMember.GetUsername(userCount));
      await _userManager.SetEmailAsync(identityUser, DeletedMember.Email);
      var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
      await _userManager.ResetPasswordAsync(identityUser, passwordToken, DeletedMember.GetPassword());
      await _userManager.UpdateSecurityStampAsync(identityUser);

      await _db.SaveChangesAsync();

      if (userIsOwner) {
      await Task.Delay(500);
      }
    }

    private bool IsUserOwner(string username, ClaimsPrincipal user) {
      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task<MemberOptionsVm> GetMemberOptionsVmAsync(string username, IPrincipal user, string returnUrl) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      var claimsPrincipalUser = user as ClaimsPrincipal;
      var isAuthorizedForProfileEdit =
        await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(username, claimsPrincipalUser);
      var isAuthorizedForProfileDelete =
        await _authorizationService.IsAuthorizedForProfileDeleteAsync(username, claimsPrincipalUser);
      var isAuthorizedProfileBlock =
        await _authorizationService.IsAuthorizedProfileBlockAsync(username, claimsPrincipalUser);
      var isAuthorizedProfileChangeRole =
        await _authorizationService.IsAuthorizedProfileChangeRoleAsync(username, claimsPrincipalUser);

      return new MemberOptionsVm {
        ReturnUrl = returnUrl,
        BlockedOn = memberFromDb.BlockedOn,
        Username = identityUser.UserName,
        IsAuthorizedForProfileDelete = isAuthorizedForProfileDelete,
        IsAuthorizedProfileBlock = isAuthorizedProfileBlock,
        IsAuthorizedForProfileEdit = isAuthorizedForProfileEdit,
        IsAuthorizedProfileChangeRole = isAuthorizedProfileChangeRole,
        IsUserOwner = IsUserOwner(username, claimsPrincipalUser)
      };
    }

    public async Task<object> GetSearchResultJsonAsync(string query) {
      var searchResult = _userManager.Users.Where(u => u.UserName.StartsWith(query))
        .Select(u => new { username = u.UserName}).ToArrayAsync();
      return await searchResult;
    }

    public Task<byte[]> GetProfileImage(string username) {
      return _db.Member.Where(m => m.IdNavigation.UserName == username)
        .Select(m => m.ProfileImage).FirstOrDefaultAsync();
    }

    public async Task<NavbarVm> GetNavbarVmAsync(IPrincipal user) {
      return new NavbarVm {
        IsAuthorizedForForumManagement =
          await _authorizationService.IsAuthorizedForForumManagementAsync(user as ClaimsPrincipal)
      };
    }
  }
}