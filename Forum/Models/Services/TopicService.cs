using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Data.Entities.Forum;
using Forum.Models.TopicViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
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

      if (currentUserId == null)
        return;

      var topic = new Topic {
        CreatedBy = currentUserId,
        ContentText = topicCreateVM.CreatedText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Topic.Add(topic);
      await _db.SaveChangesAsync();
    }
  }
}
