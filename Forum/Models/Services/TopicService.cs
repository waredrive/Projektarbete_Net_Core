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

    public async Task<TopicsIndexVm> GetTopicsIndexVm(ClaimsPrincipal user) {
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
        MostActiveMember = _userManager.FindByIdAsync(mostActiveMemberId).Result.UserName,
        NewestMember = _userManager.FindByIdAsync(newestMemberId).Result
          .UserName,
        IsAuthorizedForTopicCreate = await _authorizationService.IsAuthorizedForCreateTopic(user)
      };

      var topics = await _db.Topic.Include(t => t.Thread).ToListAsync();

      foreach (var topic in topics) topicsIndexVm.Topics.Add(await GetThreadsIndexThreadVmAsync(topic, user));

      topicsIndexVm.LatestThreads.AddRange(_db.Thread.OrderByDescending(t => t.CreatedOn).Take(10).Select(t =>
        new TopicsIndexThreadVm {
          ThreadId = t.Id,
          CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
          CreatedOn = t.CreatedOn,
          ThreadText = t.ContentText
        }));

      topicsIndexVm.LatestPosts.AddRange(_db.Post.OrderByDescending(p => p.CreatedOn).Take(10).Select(p =>
        new TopicsIndexPostVm {
          PostId = p.Id,
          ThreadId = p.Thread,
          ThreadText = p.ThreadNavigation.ContentText,
          LatestCommentTime = p.CreatedOn,
          LatestCommenter = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName
        }));

      return topicsIndexVm;
    }

    private async Task<TopicsIndexTopicVm> GetThreadsIndexThreadVmAsync(Topic topic, ClaimsPrincipal user) {
      var isAuthorizedForTopicEditBlockAndDelete =
        await _authorizationService.IsAuthorizedForTopicEditLockAndDelete(topic, user);
      var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);

      return new TopicsIndexTopicVm {
        LatestActiveThread = _db.Post.Where(p => p.ThreadNavigation.Topic == topic.Id)
          .OrderByDescending(p => p.CreatedOn)
          .Take(1).Select(p => new TopicsIndexThreadVm {
            ThreadId = p.Thread,
            ThreadText = p.ThreadNavigation.ContentText,
            CreatedOn = p.CreatedOn,
            CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName
          }).FirstOrDefault(),
        TopicId = topic.Id,
        TopicText = topic.ContentText,
        ThreadCount = topic.Thread.Count,
        PostCount = topic.Thread.Select(tt => tt.Post.Count).Sum(),
        LockedBy = lockedBy?.UserName,
        IsAuthorizedForTopicEditBlockAndDelete = isAuthorizedForTopicEditBlockAndDelete
      };
    }

    public async Task<TopicEditVm> GetTopicCreateVm(int id) {
      return await _db.Topic.Where(t => t.Id == id).Select(t => new TopicEditVm {
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Update(TopicEditVm topicEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FindAsync(topicEditVm.TopicId);
      topicFromDb.ContentText = topicEditVm.TopicText;
      topicFromDb.EditedBy = currentUserId;
      topicFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicDeleteVm> GetTopicDeleteVm(int id) {
      return await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).Select(t => new TopicDeleteVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == t.Id),
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Remove(TopicDeleteVm topicDeleteVm) {
      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicDeleteVm.TopicId);
      using (var transaction = _db.Database.BeginTransaction()) {
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

    public async Task<TopicLockVm> GetTopicLockVm(int id) {
      return await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).Select(t => new TopicLockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == t.Id),
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Lock(TopicLockVm topicLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicLockVm.TopicId);
      topicFromDb.LockedBy = currentUserId;
      topicFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicUnlockVm> GetTopicUnlockVm(int id) {
      return await _db.Topic.Include(t => t.Thread).Where(t => t.Id == id).Select(t => new TopicUnlockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == t.Id),
        LockedBy = _userManager.FindByIdAsync(t.LockedBy).Result.UserName,
        LockedOn = (DateTime) t.LockedOn,
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Unlock(TopicUnlockVm topicUnlockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicUnlockVm.TopicId);
      topicFromDb.LockedBy = null;
      topicFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public bool IsTopicLocked(int id) {
      return _db.Topic.Where(t => t.Id == id).Any(p => p.LockedBy != null);
    }

    public bool DoesTopicExist(int id) {
      return _db.Topic.Any(t => t.Id == id);
    }
  }
}