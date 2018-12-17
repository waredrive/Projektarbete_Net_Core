using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Data.Entities.Forum;
using Forum.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class AccountService {
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ForumDbContext _db;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ForumDbContext db) {
      _userManager = userManager;
      _signInManager = signInManager;
      _db = db;
    }

    public async Task<IdentityResult> Add(RegisterViewModel registerVM) {
      var user = new IdentityUser {
        Email = registerVM.Email,
        UserName = registerVM.UserName
      };

      var result = await _userManager.CreateAsync(user, registerVM.Password);

      if (!result.Succeeded)
        return result;

      try {
        var member = new Member {
          Id = user.Id,
          BirthDate = registerVM.Birthdate,
          FirstName = registerVM.FirstName,
          LastName = registerVM.LastName,
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

    public async Task<SignInResult> Login(LoginViewModel loginVM) {
      return await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, loginVM.RememberMe, false);
    }

    public async Task SignOut() {
      await _signInManager.SignOutAsync();
    }
  }
}
