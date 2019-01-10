using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.Identity;
using Forum.Models.ViewModels.AccountViewModels;
using Forum.Validations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class AccountService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SharedService _sharedService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService,
      SharedService sharedService) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
      _sharedService = sharedService;
    }

    public AccountRegisterVm GetAccountRegisterVm() {
      return new AccountRegisterVm {
        Birthdate = DateTime.UtcNow
      };
    }

    public async Task<IdentityResult> AddAsync(AccountRegisterVm accountRegisterVm) {
      if (_sharedService.IsDeletedMember(accountRegisterVm.UserName))
        return IdentityResult.Failed(new IdentityError {Description = "Forbidden username"});

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
          ProfileImage = await _sharedService.GetDefaultProfileImage(),
          Id = user.Id,
          BirthDate = accountRegisterVm.Birthdate,
          FirstName = accountRegisterVm.FirstName.Trim(),
          LastName = accountRegisterVm.LastName.Trim(),
          CreatedOn = DateTime.UtcNow
        };

        _db.Member.Add(member);
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

      if (_sharedService.IsDeletedMember(accountLoginVm.UserName))
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

    public Task<AccountEditVm> GetAccountEditVm(ClaimsPrincipal user) {
      return _db.Member.Where(m => m.IdNavigation.UserName == user.Identity.Name).Select(m => new AccountEditVm {
        Username = m.IdNavigation.UserName,
        Birthdate = m.BirthDate,
        Email = m.IdNavigation.Email,
        FirstName = m.FirstName,
        LastName = m.LastName
      }).FirstOrDefaultAsync();
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
        memberFromDb.FirstName = accountEditVm.FirstName.Trim();
        memberFromDb.LastName = accountEditVm.LastName.Trim();
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
        UserName = identityUser.UserName,
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