using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Identity;
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
      var mostActiveMembersUsername = await _db.Member.Include(m => m.IdNavigation).OrderByDescending(m =>
          m.PostCreatedByNavigation.Count + m.ThreadCreatedByNavigation.Count + m.TopicCreatedByNavigation.Count).Where(m => !IsDeletedMember(m.IdNavigation.UserName)).Select(m => m.IdNavigation.UserName).FirstOrDefaultAsync();

      var newestMemberUserName = await _db.Member.Include(m => m.IdNavigation).OrderByDescending(m => m.CreatedOn).Select(m =>m.IdNavigation.UserName).FirstOrDefaultAsync();

      return new FooterVm {
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        MostActiveMember = mostActiveMembersUsername,
        NewestMember = newestMemberUserName
      };
    }

    public bool IsDeletedMember(string username) {
      return string.Equals(username, DeletedMember.Username, StringComparison.CurrentCultureIgnoreCase);
    }
  }
}
