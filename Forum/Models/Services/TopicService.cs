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

      _db.Topic.Add(topic);
      await _db.SaveChangesAsync();
    }

    public async Task<TopicsIndexVm> GetTopicsIndexVmAsync(ClaimsPrincipal user) {
      var topicsIndexVm = new TopicsIndexVm {
        Topics = new List<TopicsIndexTopicVm>(),
        LatestThreads = new List<TopicsIndexLatestThreadVm>(),
        LatestPosts = new List<TopicsIndexPostVm>(),
        IsAuthorizedForTopicCreate = await _authorizationService.IsAuthorizedForCreateTopicAsync(user)
      };

      topicsIndexVm.Topics.AddRange(_db.Topic.Select(t => new TopicsIndexTopicVm {
        LatestThreadPostedTo = t.Thread.SelectMany(th => th.Post).OrderByDescending(p => p.CreatedOn).Select(th =>
          new TopicsIndexThreadVm {
            ThreadId = th.ThreadNavigation.Id,
            ThreadText = th.ThreadNavigation.ContentText,
            LatestCommenter = th.CreatedByNavigation.IdNavigation.UserName,
            LatestCommentTime = th.CreatedOn
          }).FirstOrDefault(),
        TopicId = t.Id,
        TopicText = t.ContentText,
        ThreadCount = t.Thread.Count,
        PostCount = t.Thread.SelectMany(p => p.Post).Count(),
        LockedBy = t.LockedByNavigation.IdNavigation.UserName
      }));

      topicsIndexVm.LatestThreads.AddRange(_db.Thread.OrderByDescending(t => t.CreatedOn).Take(10).Select(t =>
        new TopicsIndexLatestThreadVm {
          ThreadId = t.Id,
          ThreadText = t.ContentText,
          CreatedOn = t.CreatedOn,
          CreatedBy = t.CreatedByNavigation.IdNavigation.UserName
        }));

      topicsIndexVm.LatestPosts.AddRange(_db.Post.OrderByDescending(p => p.CreatedOn).Take(10).Select(p => 
        new TopicsIndexPostVm {
          PostId = p.Id,
          ThreadId = p.Thread,
          ThreadText = p.ThreadNavigation.ContentText,
          LatestCommentTime = p.CreatedOn,
          LatestCommenter = p.CreatedByNavigation.IdNavigation.UserName
        }
      ));

      return topicsIndexVm;
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

    public Task<TopicDeleteVm> GetTopicDeleteVmAsync(int id) {
      return _db.Topic.Where(t => t.Id == id).Select(t => new TopicDeleteVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = t.Thread.SelectMany(th => th.Post).Count(),
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
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

    public Task<TopicLockVm> GetTopicLockVmAsync(int id) {
      return _db.Topic.Where(t => t.Id == id).Select(t => new TopicLockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = t.Thread.SelectMany(th => th.Post).Count(),
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
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

    public Task<TopicUnlockVm> GetTopicUnlockVmAsync(int id) {
      return _db.Topic.Where(t => t.Id == id).Select(t => new TopicUnlockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        ThreadCount = t.Thread.Count,
        PostCount = t.Thread.SelectMany(th => th.Post).Count(),
        LockedBy = t.LockedByNavigation.IdNavigation.UserName,
        LockedOn = t.LockedOn,
        TopicId = t.Id,
        TopicText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UnlockAsync(TopicUnlockVm topicUnlockVm) {
      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicUnlockVm.TopicId);
      topicFromDb.LockedBy = null;
      topicFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public async Task<TopicOptionsVm> GetTopicOptionsVmAsync(int topicId, IPrincipal user, string returnUrl) {
      var isAuthorizedForTopicEditLockAndDelete =
        await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(topicId, user as ClaimsPrincipal);
      return await _db.Topic.Where(t => t.Id == topicId).Select(t => new TopicOptionsVm {
        ReturnUrl = returnUrl,
        LockedOn = t.LockedOn,
        TopicId = t.Id,
        IsAuthorizedForTopicEditLockAndDelete = isAuthorizedForTopicEditLockAndDelete
      }).FirstOrDefaultAsync();
    }

    public Task<bool> IsTopicLocked(int id) {
      return _db.Topic.Where(t => t.Id == id).AnyAsync(p => p.LockedBy != null);
    }

    public Task<bool> DoesTopicExist(int id) {
      return _db.Topic.AnyAsync(t => t.Id == id);
    }
  }
}