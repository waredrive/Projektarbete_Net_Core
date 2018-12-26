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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService,
      IHostingEnvironment env) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
      _env = env;
    }

    public AccountRegisterVm GetAccountRegisterVm() {
      return new AccountRegisterVm {
        Birthdate = DateTime.UtcNow
      };
    }

    public async Task<IdentityResult> Add(AccountRegisterVm accountRegisterVm) {
      await CreateRoles();

      var identityError = new IdentityError().Description = "You must be older than 13 to use this Forum.";

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
            File.ReadAllBytes(_env.WebRootFileProvider.GetFileInfo("img/profile/default_profile.jpg")?.PhysicalPath),
          Id = user.Id,
          BirthDate = accountRegisterVm.Birthdate,
          FirstName = accountRegisterVm.FirstName,
          LastName = accountRegisterVm.LastName,
          CreatedOn = DateTime.UtcNow
        };

        _db.Member.Add(member);
        await _db.SaveChangesAsync();
      } catch (Exception) {
        await _userManager.DeleteAsync(user);
        throw;
      }

      return result;
    }

    public async Task<SignInResult> Login(AccountLoginVm accountLoginVm) {
      if (!DoesAccountExist(accountLoginVm.UserName))
        return SignInResult.Failed;

        if (await _authorizationService.IsProfileInternal(accountLoginVm.UserName))
        return SignInResult.Failed;

      var result = await _signInManager.PasswordSignInAsync(accountLoginVm.UserName, accountLoginVm.Password,
        accountLoginVm.RememberMe, false);
      if (result.Succeeded)
        await ResetOldBlockStatus(accountLoginVm.UserName);
      return result;
    }

    private async Task ResetOldBlockStatus(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      if (memberFromDb.BlockedEnd < DateTime.UtcNow) {
        memberFromDb.BlockedBy = null;
        memberFromDb.BlockedOn = null;
        memberFromDb.BlockedEnd = null;
        await _db.SaveChangesAsync();
      }
    }

    public async Task SignOut() {
      await _signInManager.SignOutAsync();
    }

    public async Task<AccountEditVm> GetAccountEditVm(ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new AccountEditVm {
        Birthdate = memberFromDb.BirthDate,
        Email = identityUser.Email,
        FirstName = memberFromDb.FirstName,
        LastName = memberFromDb.LastName
      };
    }

    public async Task<IdentityResult> UpdateAccount(AccountEditVm accountEditVm, ClaimsPrincipal user) {
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
      } catch (Exception) {
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

    public async Task<AccountDetailsVm> GetAccountDetailsVm(string username, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

      return new AccountDetailsVm {
        Birthdate = memberFromDb.BirthDate,
        Email = identityUser.Email,
        FirstName = memberFromDb.FirstName,
        LastName = memberFromDb.LastName,
        IsAuthorizedForAccountEdit =
          await _authorizationService.IsAuthorizedForAccountAndProfileEdit(identityUser.UserName, user)
      };
    }

    private async Task CreateRoles() {
      var roles = typeof(Roles).GetFields(BindingFlags.Static | BindingFlags.Public)
        .Where(x => x.IsLiteral && !x.IsInitOnly)
        .Select(x => x.GetValue(null)).Cast<string>();

      foreach (var role in roles) {
        if (await _roleManager.RoleExistsAsync(role))
          continue;

        var roleToAdd = new IdentityRole { Name = role };
        await _roleManager.CreateAsync(roleToAdd);
      }
    }

    public ValidationResult HasMinimumAllowedAge(DateTime birthdate) {
      var result = new ValidationResult();
      if (birthdate > DateTime.UtcNow.AddYears(-13))
        result.Errors.Add("You must be at least 13 years old to use this forum.");

      return result;
    }

    public bool DoesAccountExist(string username) {
      return _userManager.FindByNameAsync(username).Result != null;
    }
  }
}