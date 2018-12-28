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
    private readonly ForumDbContext _db;

    public SharedService(
      UserManager<IdentityUser> userManager, ForumDbContext db) {
      _userManager = userManager;
      _db = db;
    }

    public async Task<string> GetProfileImageStringByUsernameAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      return await GetProfileImageStringByMemberIdAsync(identityUser.Id);
    }

    public async Task<string> GetProfileImageStringByMemberIdAsync(string id) {
      var imageByteArr = await _db.Member.Where(m => m.Id == id).Select(m => m.ProfileImage).FirstOrDefaultAsync();
      return imageByteArr != null ? Convert.ToBase64String(imageByteArr) : null;
    }

    public bool DoesUserAccountExist(string username) {
      return _userManager.FindByNameAsync(username).Result != null;
    }
  }
}
