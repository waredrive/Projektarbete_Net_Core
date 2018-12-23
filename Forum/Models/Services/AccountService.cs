using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Forum.Models.Services {
  public class AccountService {
    private readonly ForumDbContext _db;
    private readonly AuthorizationService _authorizationService;
    private readonly IHostingEnvironment _env;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService, IHostingEnvironment env) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
      _env = env;
    }

    public async Task<IdentityResult> Add(AccountRegisterVm accountRegisterVm) {
      await CreateRoles();
      var user = new IdentityUser {
        Email = accountRegisterVm.Email,
        UserName = accountRegisterVm.UserName
      };

      var result = await _userManager.CreateAsync(user, accountRegisterVm.Password);

      if (!result.Succeeded)
        return result;

      try {
        await _userManager.AddToRoleAsync(user, Roles.User);
        var profilePicture = File.ReadAllBytes(_env.WebRootFileProvider.GetFileInfo("img/profile/default_profile.jpg")?.PhysicalPath);

        var member = new Member {
          ProfileImage = profilePicture,
          Id = user.Id,
          BirthDate = accountRegisterVm.Birthdate,
          FirstName = accountRegisterVm.FirstName,
          LastName = accountRegisterVm.LastName,
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

    public async Task<SignInResult> Login(AccountLoginVm accountLoginVm) {
      return await _signInManager.PasswordSignInAsync(accountLoginVm.UserName, accountLoginVm.Password, accountLoginVm.RememberMe, false);
    }

    public async Task SignOut() {
      await _signInManager.SignOutAsync();
    }

    private async Task CreateRoles() {
      var adminExist = await _roleManager.RoleExistsAsync(Roles.Admin);
      if (!adminExist) {
        var role = new IdentityRole {Name = Roles.Admin};
        await _roleManager.CreateAsync(role);
      }

      var moderatorExist = await _roleManager.RoleExistsAsync(Roles.Moderator);
      if (!moderatorExist) {
        var role = new IdentityRole {Name = Roles.Moderator};
        await _roleManager.CreateAsync(role);
      }

      var userExist = await _roleManager.RoleExistsAsync(Roles.User);
      if (!userExist) {
        var role = new IdentityRole {Name = Roles.User};
        await _roleManager.CreateAsync(role);
      }
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

    public async Task<IdentityResult> UpdatePassword(AccountPasswordEditVm accountPasswordEditVm, ClaimsPrincipal user) {
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
        IsAuthorizedForAccountEdit = _authorizationService.IsAuthorizedForAccountAndPasswordEdit(identityUser.UserName, user)
      };
    }
  }
}