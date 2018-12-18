using System;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Identity;

namespace Forum.Models.Services {
  public class AccountService {
    private readonly ForumDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
    }

    public async Task<IdentityResult> Add(RegisterViewModel registerVM) {
      await CreateRoles();
      var user = new IdentityUser {
        Email = registerVM.Email,
        UserName = registerVM.UserName
      };

      var result = await _userManager.CreateAsync(user, registerVM.Password);

      if (!result.Succeeded)
        return result;

      try {
        await _userManager.AddToRoleAsync(user, Roles.User);

        var member = new Member {
          Id = user.Id,
          BirthDate = registerVM.Birthdate,
          FirstName = registerVM.FirstName,
          LastName = registerVM.LastName,
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

    public async Task<SignInResult> Login(LoginViewModel loginVM) {
      return await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, loginVM.RememberMe, false);
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
  }
}