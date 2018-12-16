using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;

namespace Forum.MVC.Models.Services {
  public class AccountService {
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public async Task<IdentityResult> Add(RegisterViewModel registerVM) {
      var user = new IdentityUser {
        Email = registerVM.Email,
        UserName = registerVM.UserName
      };

      return await _userManager.CreateAsync(user, registerVM.Password);
    }

    public async Task<SignInResult> Login(LoginViewModel loginVM) {
      return await _signInManager.PasswordSignInAsync(loginVM.UserName, loginVM.Password, loginVM.RememberMe, false);
    }

    public async Task SignOut() {
      await _signInManager.SignOutAsync();
    }
  }
}
