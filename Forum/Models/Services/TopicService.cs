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
        ContentText = topicCreateVm.TopicText,
        CreatedOn = DateTime.UtcNow
      };

      await _db.Topic.AddAsync(topic);
      await _db.SaveChangesAsync();
    }

    public async Task<TopicsIndexVm> GetTopicsIndexVmAsync(ClaimsPrincipal user) {
      var mostActiveMemberId = _db.Member.Where(m => !m.IsInternal).OrderByDescending(m =>
          m.PostCreatedByNavigation.Count + m.ThreadCreatedByNavigation.Count + m.TopicCreatedByNavigation.Count)
        .Select(m => m.Id).First();
      var newestMemberId = _db.Member.OrderByDescending(m => m.CreatedOn).First().Id;

      var topicsIndexVm = new TopicsIndexVm {
        Topics = new List<TopicsIndexTopicVm>(),
        LatestThreads = new List<TopicsIndexThreadVm>(),
        LatestPosts = new List<TopicsIndexPostVm>(),
        TotalMembers = _userManager.Users.Count(),
        TotalPosts = _db.Post.Count(),
        MostActiveMember = (await _userManager.FindByIdAsync(mostActiveMemberId)).UserName,
        NewestMember = (await _userManager.FindByIdAsync(newestMemberId)).UserName,
        IsAuthorizedForTopicCreate = await _authorizationService.IsAuthorizedForCreateTopicAsync(user)
      };

      var topics = await _db.Topic.Include(t => t.Thread).ToListAsync();
      foreach (var topic in topics)
        topicsIndexVm.Topics.Add(await GetTopicsIndexTopicVmAsync(topic, user));

      var latestThreads = await _db.Thread.OrderByDescending(t => t.CreatedOn).Take(10).ToListAsync();
      foreach (var thread in latestThreads) {
        topicsIndexVm.LatestThreads.Add(await GetTopicIndexThreadVmAsync(thread));
      }

      var latestPosts = await _db.Post.OrderByDescending(p => p.CreatedOn).Take(10).ToListAsync();
      foreach (var post in latestPosts) {
        topicsIndexVm.LatestPosts.Add(await GetTopicIndexPostVmAsync(post));
      }

      return topicsIndexVm;
    }

    private async Task<TopicsIndexTopicVm> GetTopicsIndexTopicVmAsync(Topic topic, ClaimsPrincipal user) {
      var isAuthorizedForTopicEditLockAndDelete =
        await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(topic, user);
      var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);
      var mostRecentThreadPostedTo = await _db.Post.Where(p => p.ThreadNavigation.Topic == topic.Id)
        .OrderByDescending(p => p.CreatedOn)
        .Take(1).Select(p => p.ThreadNavigation).FirstOrDefaultAsync();

      return new TopicsIndexTopicVm {
        LatestActiveThread = mostRecentThreadPostedTo != null ? await GetTopicIndexThreadVmAsync(mostRecentThreadPostedTo) : null,
        TopicId = topic.Id,
        TopicText = topic.ContentText,
        ThreadCount = topic.Thread.Count,
        PostCount = topic.Thread.Select(tt => tt.Post.Count).Sum(),
        LockedBy = lockedBy?.UserName,
        IsAuthorizedForTopicEditLockAndDelete = isAuthorizedForTopicEditLockAndDelete
      };
    }

    private async Task<TopicsIndexThreadVm> GetTopicIndexThreadVmAsync(Thread thread) {
      var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);

      return new TopicsIndexThreadVm {
        ThreadId = thread.Id,
        ThreadText = thread.ContentText,
        CreatedOn = thread.CreatedOn,
        CreatedBy = createdBy.UserName
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
      topicFromDb.ContentText = topicEditVm.TopicText;
      topicFromDb.EditedBy = currentUserId;
      topicFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicDeleteVm>GetTopicDeleteVmAsync(int id) {
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
        } catch (Exception) {
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

      return  new TopicUnlockVm {
        CreatedOn = topic.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadCount = topic.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == topic.Id),
        LockedBy = lockedBy.UserName,
        LockedOn = (DateTime)topic.LockedOn,
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

    public Task<bool> IsTopicLocked(int id) {
      return _db.Topic.Where(t => t.Id == id).AnyAsync(p => p.LockedBy != null);
    }

    public Task<bool> DoesTopicExist(int id) {
      return _db.Topic.AnyAsync(t => t.Id == id);
    }
  }
}