using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        LatestPosts = new List<TopicsIndexPostVm>(),
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        NewestMember = _userManager.FindByIdAsync(_db.Member.OrderByDescending(m => m.CreatedOn).First().Id).Result.UserName,
      };

     topicsIndexVm.Topics.AddRange(_db.Topic.Include(t => t.Thread).Select(t => new TopicsIndexTopicVm {
        LatestActiveThread = _db.Post.Where(p => p.ThreadNavigation.Topic == t.Id).OrderByDescending(p => p.CreatedOn).Take(1).Select(p => new TopicsIndexThreadVm {
          ThreadId = p.Thread,
          ThreadText = p.ThreadNavigation.ContentText,
          CreatedOn = p.CreatedOn,
          CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName
        }).FirstOrDefault(),
        TopicId = t.Id,
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        TopicText = t.ContentText,
        ThreadCount = t.Thread.Count,
        PostCount = t.Thread.Select(tt => tt.Post.Count).Sum()
      }));

      topicsIndexVm.LatestThreads.AddRange(_db.Thread.OrderByDescending(t => t.CreatedOn).Take(10).Select( t => new TopicsIndexThreadVm {
        ThreadId = t.Id,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        CreatedOn = t.CreatedOn,
        ThreadText = t.ContentText
      }));

      topicsIndexVm.LatestPosts.AddRange(_db.Post.OrderByDescending(p => p.CreatedOn).Take(10).Select(p => new TopicsIndexPostVm {
        ThreadId = p.Thread,
       ThreadText = p.ThreadNavigation.ContentText,
        LatestCommentTime = p.CreatedOn,
        LatestCommenter = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName
      }));

      return topicsIndexVm;
    }
  }
}