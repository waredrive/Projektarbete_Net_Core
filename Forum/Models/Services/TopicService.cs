using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Identity;

namespace Forum.Models.Services {
  public class TopicService {
    private readonly ForumDbContext _db;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public TopicService(ForumDbContext db, UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager) {
      _db = db;
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public async Task Add(TopicCreateVm topicCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topic = new Topic {
        CreatedBy = currentUserId,
        ContentText = topicCreateVm.TopicText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Topic.Add(topic);
      await _db.SaveChangesAsync();
    }

    public async Task<TopicsIndexVm> GetTopicsIndexVm() {
      var topicsIndexVm = new TopicsIndexVm {
        Topics = new List<TopicsIndexTopicVm>(),
        LatestThreads = new List<TopicsIndexThreadVm>(),
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        NewestMember = _userManager.FindByIdAsync(_db.Member.OrderByDescending(m => m.CreatedOn).First().Id).Result.UserName
      };

      topicsIndexVm.Topics.AddRange(_db.Topic.Select(t => new TopicsIndexTopicVm {
        TopicId = t.Id,
        CreatedOn = (DateTime)t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        TopicText = t.ContentText
      }));

      topicsIndexVm.LatestThreads.AddRange(_db.Thread.OrderBy(t => t.CreatedOn).Take(10).Select( t => new TopicsIndexThreadVm {
        ThreadId = t.Id,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        CreatedOn = (DateTime)t.CreatedOn,
        ThreadText = t.ContentText
      }));

      return topicsIndexVm;
    }
  }
}