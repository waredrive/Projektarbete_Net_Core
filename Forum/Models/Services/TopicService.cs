using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.MVC.Models.TopicViewModels;
using Forum.Persistence.Entities.ForumData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.MVC.Models.Services {
  public class TopicService {
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public TopicService(ForumDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) {
      _db = db;
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public async Task Add(TopicCreateVM topicCreateVM, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      var currentAccount = _db.Account.Include(a => a.Member).Include(a => a.RoleNavigation)
        .SingleOrDefault(a => a.Member.Any(m => m.Id == currentUserId));

      if (currentAccount?.Id == null || currentAccount.RoleNavigation.Id != 1)
        return;

      var topic = new Topic {
        CreatedBy = currentAccount.Id,
        ContentText = topicCreateVM.CreatedText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Topic.Add(topic);
      await _db.SaveChangesAsync();
    }
  }
}
