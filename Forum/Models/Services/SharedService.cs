using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.ViewModels.ComponentViewModels.FooterViewModels;
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
      if (!DoesUserAccountExist(username))
        return null;

      var identityUser = await _userManager.FindByNameAsync(username);
      return await GetProfileImageStringByMemberIdAsync(identityUser.Id);
    }

    public async Task<string> GetProfileImageStringByMemberIdAsync(string id) {
      var imageByteArr = await _db.Member.Where(m => m.Id == id).Select(m => m.ProfileImage).FirstOrDefaultAsync();
      return imageByteArr != null ? Convert.ToBase64String(imageByteArr) : null;
    }

    public bool DoesUserAccountExist(string username) {
      if (username == null)
        return false;
      return _userManager.FindByNameAsync(username)?.Result != null;
    }

    public async Task<FooterVm> GetFooterVmAsync() {
      var mostActiveMemberId = _db.Member.Where(m => !m.IsInternal).OrderByDescending(m =>
          m.PostCreatedByNavigation.Count + m.ThreadCreatedByNavigation.Count + m.TopicCreatedByNavigation.Count)
        .Select(m => m.Id).First();
      var newestMemberId = _db.Member.OrderByDescending(m => m.CreatedOn).First().Id;

      return new FooterVm {
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        MostActiveMember = (await _userManager.FindByIdAsync(mostActiveMemberId)).UserName,
        NewestMember = (await _userManager.FindByIdAsync(newestMemberId)).UserName
      };
    }
  }
}
