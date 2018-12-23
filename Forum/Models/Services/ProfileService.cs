using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Context;
using Microsoft.AspNetCore.Identity;

namespace Forum.Models.Services {
  public class ProfileService {
    private readonly ForumDbContext _db;
    private readonly AuthorizationService _authorizationService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ProfileService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
    }

    //<div class="form-group">
    //  <img src = "data:image;base64,@Model.Image" />
    //</ div >


    //public async Task<AccountDetailsVm> GetAccountDetailsVm(string username, ClaimsPrincipal user) {
    //  var identityUser = await _userManager.FindByNameAsync(username);
    //  var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);

    //  return new AccountDetailsVm {
    //    Image = memberFromDb.ProfileImage,
    //    Birthdate = memberFromDb.BirthDate,
    //    Email = identityUser.Email,
    //    FirstName = memberFromDb.FirstName,
    //    LastName = memberFromDb.LastName,
    //    IsAuthorizedForAccountEdit = _authorizationService.IsAuthorizedForAccountAndPasswordEdit(identityUser.UserName, user)
    //  };
    //}
  }
}
