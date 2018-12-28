using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.AccountViewModels;
using Forum.Validations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class AccountService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly IHostingEnvironment _env;
    private readonly SharedService _sharedService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService,
      IHostingEnvironment env, SharedService sharedService) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
      _env = env;
      _sharedService = sharedService;
    }

    public AccountRegisterVm GetAccountRegisterVm() {
      return new AccountRegisterVm {
        Birthdate = DateTime.UtcNow
      };
    }

    public async Task<IdentityResult> AddAsync(AccountRegisterVm accountRegisterVm) {
      await CreateRolesAsync();

      var user = new IdentityUser {
        Email = accountRegisterVm.Email,
        UserName = accountRegisterVm.UserName
      };

      var result = await _userManager.CreateAsync(user, accountRegisterVm.Password);

      if (!result.Succeeded)
        return result;

      try {
        await _userManager.AddToRoleAsync(user, Roles.User);

        var member = new Member {
          ProfileImage =
            await File.ReadAllBytesAsync(_env.WebRootFileProvider.GetFileInfo("img/profile/default_profile.jpg")?.PhysicalPath),
          Id = user.Id,
          BirthDate = accountRegisterVm.Birthdate,
          FirstName = accountRegisterVm.FirstName,
          LastName = accountRegisterVm.LastName,
          CreatedOn = DateTime.UtcNow
        };

        await _db.Member.AddAsync(member);
        await _db.SaveChangesAsync();
      }
      catch (Exception) {
        await _userManager.DeleteAsync(user);
        throw;
      }

      return result;
    }

    public async Task<SignInResult> LoginAsync(AccountLoginVm accountLoginVm) {
      if (!_sharedService.DoesUserAccountExist(accountLoginVm.UserName))
        return SignInResult.Failed;

      if (await _authorizationService.IsProfileInternalAsync(accountLoginVm.UserName))
        return SignInResult.Failed;

      var result = await _signInManager.PasswordSignInAsync(accountLoginVm.UserName, accountLoginVm.Password,
        accountLoginVm.RememberMe, false);
      if (result.Succeeded)
        await ResetOldBlockStatusAsync(accountLoginVm.UserName);
      return result;
    }

    private async Task ResetOldBlockStatusAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      if (memberFromDb.BlockedEnd > DateTime.UtcNow) {
        memberFromDb.BlockedBy = null;
        memberFromDb.BlockedOn = null;
        memberFromDb.BlockedEnd = null;
        await _db.SaveChangesAsync();
      }
    }

    public Task SignOut() {
      return _signInManager.SignOutAsync();
    }

    public async Task<AccountEditVm> GetAccountEditVmAsync(ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new AccountEditVm {
        Birthdate = memberFromDb.BirthDate,
        Email = identityUser.Email,
        FirstName = memberFromDb.FirstName,
        LastName = memberFromDb.LastName
      };
    }

    public async Task<IdentityResult> UpdateAccountAsync(AccountEditVm accountEditVm, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var oldEmail = identityUser.Email;
      var result = await _userManager.SetEmailAsync(identityUser, accountEditVm.Email);

      if (!result.Succeeded)
        return result;

      try {
        var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
        memberFromDb.BirthDate = accountEditVm.Birthdate;
        memberFromDb.FirstName = accountEditVm.FirstName;
        memberFromDb.LastName = accountEditVm.LastName;
        await _db.SaveChangesAsync();
      }
      catch (Exception) {
        await _userManager.SetEmailAsync(identityUser, oldEmail);
        throw;
      }

      return result;
    }

    public async Task<IdentityResult>
      UpdatePassword(AccountPasswordEditVm accountPasswordEditVm, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      return await _userManager.ChangePasswordAsync(identityUser, accountPasswordEditVm.OldPassword,
        accountPasswordEditVm.NewPassword);
    }

    public async Task<AccountDetailsVm> GetAccountDetailsVmAsync(string username, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new AccountDetailsVm {
        Birthdate = memberFromDb.BirthDate,
        Email = identityUser.Email,
        FirstName = memberFromDb.FirstName,
        LastName = memberFromDb.LastName,
        IsAuthorizedForAccountEdit =
          await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(identityUser.UserName, user)
      };
    }

    private async Task CreateRolesAsync() {
      var roles = typeof(Roles).GetFields(BindingFlags.Static | BindingFlags.Public)
        .Where(x => x.IsLiteral && !x.IsInitOnly)
        .Select(x => x.GetValue(null)).Cast<string>();

      foreach (var role in roles) {
        if (await _roleManager.RoleExistsAsync(role))
          continue;

        var roleToAdd = new IdentityRole {Name = role};
        await _roleManager.CreateAsync(roleToAdd);
      }
    }

    public CustomValidationResult HasMinimumAllowedAge(DateTime birthdate) {
      var result = new CustomValidationResult();
      if (birthdate > DateTime.UtcNow.AddYears(-13))
        result.Errors.Add("You must be at least 13 years old to use this forum.");

      return result;
    }
  }
}