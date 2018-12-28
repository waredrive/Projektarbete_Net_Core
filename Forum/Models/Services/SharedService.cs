using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class SharedService {
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ForumDbContext _db;
    private readonly AuthorizationService _authorizationService;

    public SharedService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService,
      IHostingEnvironment env) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
    }

    public async Task<string> GetProfileImageStringByUsername(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      return await GetProfileImageStringByMemberId(identityUser.Id);
    }

    public async Task<string> GetProfileImageStringByMemberId(string id) {
      var imageByteArr = await _db.Member.Where(m => m.Id == id).Select(m => m.ProfileImage).FirstOrDefaultAsync();
      return imageByteArr != null ? Convert.ToBase64String(imageByteArr) : null;
    }

    public bool DoesUserAccountExist(string username) {
      return _userManager.FindByNameAsync(username).Result != null;
    }
  }
}
