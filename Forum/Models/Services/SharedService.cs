using System;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Identity;
using Forum.Models.ViewModels.ComponentViewModels.FooterViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class SharedService {
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public SharedService(
      UserManager<IdentityUser> userManager, ForumDbContext db) {
      _userManager = userManager;
      _db = db;
    }

    public bool DoesUserAccountExist(string username) {
      if (username == null)
        return false;
      return _userManager.FindByNameAsync(username)?.Result != null;
    }

    public async Task<FooterVm> GetFooterVmAsync() {
      var mostActiveMembersUsername = await _db.Member.Include(m => m.IdNavigation).OrderByDescending(m =>
          m.PostCreatedByNavigation.Count + m.ThreadCreatedByNavigation.Count + m.TopicCreatedByNavigation.Count)
        .Where(m => !IsDeletedMember(m.IdNavigation.UserName)).Select(m => m.IdNavigation.UserName)
        .FirstOrDefaultAsync();

      var newestMemberUserName = await _db.Member.Include(m => m.IdNavigation).OrderByDescending(m => m.CreatedOn)
        .Select(m => m.IdNavigation.UserName).FirstOrDefaultAsync();

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