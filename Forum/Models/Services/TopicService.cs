using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ComponentViewModels.TopicOptionsViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class TopicService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public TopicService(ForumDbContext db, UserManager<IdentityUser> userManager,
      AuthorizationService authorizationService) {
      _db = db;
      _userManager = userManager;
      _authorizationService = authorizationService;
    }

    public async Task AddAsync(TopicCreateVm topicCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topic = new Topic {
        CreatedBy = currentUserId,
        ContentText = topicCreateVm.TopicText.Trim(),
        CreatedOn = DateTime.UtcNow
      };

      await _db.Topic.AddAsync(topic);
      await _db.SaveChangesAsync();
    }

    public async Task<TopicsIndexVm> GetTopicsIndexVmAsync(ClaimsPrincipal user) {
      var topicsIndexVm = new TopicsIndexVm {
        Topics = new List<TopicsIndexTopicVm>(),
        LatestThreads = new List<TopicsIndexLatestThreadVm>(),
        LatestPosts = new List<TopicsIndexPostVm>(),
        IsAuthorizedForTopicCreate = await _authorizationService.IsAuthorizedForCreateTopicAsync(user)
      };

      var topics = await _db.Topic.Include(t => t.Thread).ToListAsync();
      foreach (var topic in topics)
        topicsIndexVm.Topics.Add(await GetTopicsIndexTopicVmAsync(topic));

      var latestThreads =
        await _db.Thread.Include(t => t.Post).OrderByDescending(t => t.CreatedOn).Take(10).ToListAsync();
      foreach (var thread in latestThreads)
        topicsIndexVm.LatestThreads.Add(await GetTopicIndexLatestThreadVmAsync(thread));

      var latestPosts = await _db.Post.OrderByDescending(p => p.CreatedOn).Take(10).ToListAsync();
      foreach (var post in latestPosts) topicsIndexVm.LatestPosts.Add(await GetTopicIndexPostVmAsync(post));

      return topicsIndexVm;
    }

    private async Task<TopicsIndexTopicVm> GetTopicsIndexTopicVmAsync(Topic topic) {
      var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);
      var mostRecentPostInTopic = await _db.Post.Where(p => p.ThreadNavigation.Topic == topic.Id)
        .OrderByDescending(p => p.CreatedOn).FirstOrDefaultAsync();

      return new TopicsIndexTopicVm {
        LatestThreadPostedTo = mostRecentPostInTopic != null
          ? await GetTopicIndexThreadVmAsync(mostRecentPostInTopic)
          : null,
        TopicId = topic.Id,
        TopicText = topic.ContentText,
        ThreadCount = topic.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == topic.Id),
        LockedBy = lockedBy?.UserName
      };
    }

    private async Task<TopicsIndexLatestThreadVm> GetTopicIndexLatestThreadVmAsync(Thread thread) {
      var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);

      return new TopicsIndexLatestThreadVm {
        ThreadId = thread.Id,
        ThreadText = thread.ContentText,
        CreatedOn = thread.CreatedOn,
        CreatedBy = createdBy.UserName
      };
    }

    private async Task<TopicsIndexThreadVm> GetTopicIndexThreadVmAsync(Post mostRecentPostInTopic) {
      var latestPostInThreadCreator = await _userManager.FindByIdAsync(mostRecentPostInTopic.CreatedBy);

      return new TopicsIndexThreadVm {
        ThreadId = mostRecentPostInTopic.ThreadNavigation.Id,
        ThreadText = mostRecentPostInTopic.ThreadNavigation.ContentText,
        LatestCommenter = latestPostInThreadCreator?.UserName,
        LatestCommentTime = mostRecentPostInTopic?.CreatedOn
      };
    }

    private async Task<TopicsIndexPostVm> GetTopicIndexPostVmAsync(Post post) {
      var createdBy = await _userManager.FindByIdAsync(post.CreatedBy);

      return new TopicsIndexPostVm {
        PostId = post.Id,
        ThreadId = post.Thread,
        ThreadText = post.ThreadNavigation.ContentText,
        LatestCommentTime = post.CreatedOn,
        LatestCommenter = createdBy.UserName
      };
    }

    public Task<TopicEditVm> GetTopicCreateVm(int id) {
      return _db.Topic.Where(t => t.Id == id).Select(t => new TopicEditVm {
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(TopicEditVm topicEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FindAsync(topicEditVm.TopicId);
      topicFromDb.ContentText = topicEditVm.TopicText.Trim();
      topicFromDb.EditedBy = currentUserId;
      topicFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicDeleteVm> GetTopicDeleteVmAsync(int id) {
      var topic = await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).FirstOrDefaultAsync();
      var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);

      return new TopicDeleteVm {
        CreatedOn = topic.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadCount = topic.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == topic.Id),
        TopicId = topic.Id,
        TopicText = topic.ContentText
      };
    }

    public async Task RemoveAsync(TopicDeleteVm topicDeleteVm) {
      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicDeleteVm.TopicId);
      using (var transaction = await _db.Database.BeginTransactionAsync()) {
        try {
          _db.Post.RemoveRange(_db.Post.Where(p => p.ThreadNavigation.Topic == topicFromDb.Id));
          _db.Thread.RemoveRange(_db.Thread.Where(t => t.Topic == topicFromDb.Id));
          _db.Topic.Remove(topicFromDb);
          await _db.SaveChangesAsync();
          transaction.Commit();
        }
        catch (Exception) {
          transaction.Rollback();
        }
      }
    }

    public async Task<TopicLockVm> GetTopicLockVmAsync(int id) {
      var topic = await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).FirstOrDefaultAsync();
      var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);

      return new TopicLockVm {
        CreatedOn = topic.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadCount = topic.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == topic.Id),
        TopicId = topic.Id,
        TopicText = topic.ContentText
      };
    }

    public async Task LockAsync(TopicLockVm topicLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicLockVm.TopicId);
      topicFromDb.LockedBy = currentUserId;
      topicFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicUnlockVm> GetTopicUnlockVmAsync(int id) {
      var topic = await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).FirstOrDefaultAsync();
      var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);
      var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);

      return new TopicUnlockVm {
        CreatedOn = topic.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadCount = topic.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == topic.Id),
        LockedBy = lockedBy.UserName,
        LockedOn = (DateTime) topic.LockedOn,
        TopicId = topic.Id,
        TopicText = topic.ContentText
      };
    }

    public async Task UnlockAsync(TopicUnlockVm topicUnlockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicUnlockVm.TopicId);
      topicFromDb.LockedBy = null;
      topicFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicOptionsVm> GetTopicOptionsVmAsync(int topicId, IPrincipal user, string returnUrl) {
      var topic = await _db.Topic.Where(t => t.Id == topicId).FirstOrDefaultAsync();
      return new TopicOptionsVm {
        ReturnUrl = returnUrl,
        LockedOn = topic.LockedOn,
        TopicId = topic.Id,
        IsAuthorizedForTopicEditLockAndDelete =
          await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(topic.Id, user as ClaimsPrincipal)
      };
    }

    public Task<bool> IsTopicLocked(int id) {
      return _db.Topic.Where(t => t.Id == id).AnyAsync(p => p.LockedBy != null);
    }

    public Task<bool> DoesTopicExist(int id) {
      return _db.Topic.AnyAsync(t => t.Id == id);
    }
  }
}