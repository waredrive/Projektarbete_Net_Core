using System;
using System.IO;
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
    private readonly ForumDbContext _db;
    private readonly IHostingEnvironment _env;
    private readonly UserManager<IdentityUser> _userManager;

    public SharedService(
      UserManager<IdentityUser> userManager, ForumDbContext db, IHostingEnvironment env) {
      _userManager = userManager;
      _db = db;
      _env = env;
    }

    public bool DoesUserAccountExist(string username) {
      if (username == null)
        return false;
      return _userManager.FindByNameAsync(username)?.Result != null;
    }

    public async Task<FooterVm> GetFooterVmAsync() {
      var mostActiveMembersUsername = await _db.Member.OrderByDescending(m =>
          m.PostCreatedByNavigation.Count + m.ThreadCreatedByNavigation.Count + m.TopicCreatedByNavigation.Count)
        .Where(m => !IsDeletedMember(m.IdNavigation.UserName)).Select(m => m.IdNavigation.UserName)
        .FirstOrDefaultAsync();

      var newestMemberUserName = await _db.Member.OrderByDescending(m => m.CreatedOn)
        .Select(m => m.IdNavigation.UserName).Where(u => !IsDeletedMember(u)).FirstOrDefaultAsync();

      return new FooterVm {
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        MostActiveMember = mostActiveMembersUsername,
        NewestMember = newestMemberUserName
      };
    }

    public Task<byte[]> GetDefaultProfileImage() {
      return File.ReadAllBytesAsync(_env.WebRootFileProvider.GetFileInfo("img/profile/default_profile.jpg")
        ?.PhysicalPath);
    }

    public bool IsDeletedMember(string username) {
      return username.StartsWith(DeletedMember.UsernamePrefix, StringComparison.CurrentCultureIgnoreCase);
    }
  }
}