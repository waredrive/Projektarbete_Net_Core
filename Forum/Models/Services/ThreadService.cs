using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.Pagination;
using Forum.Models.ViewModels.ComponentViewModels.ThreadOptionsViewModels;
using Forum.Models.ViewModels.ThreadViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ThreadService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ThreadService(ForumDbContext db, AuthorizationService authorizationService,
      UserManager<IdentityUser> userManager) {
      _db = db;
      _authorizationService = authorizationService;
      _userManager = userManager;
    }

    public async Task AddAsync(ThreadCreateVm threadCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      using (var transaction = await _db.Database.BeginTransactionAsync()) {
        try {
          var thread = new Thread {
            Topic = threadCreateVm.TopicId,
            CreatedBy = currentUserId,
            ContentText = threadCreateVm.ThreadText.Trim(),
            CreatedOn = DateTime.UtcNow
          };

          await _db.Thread.AddAsync(thread);

          var post = new Post {
            Thread = thread.Id,
            CreatedBy = currentUserId,
            ContentText = threadCreateVm.Post.PostText.Trim(),
            CreatedOn = DateTime.UtcNow
          };

          await _db.Post.AddAsync(post);
          await _db.SaveChangesAsync();
          transaction.Commit();
        }
        catch (Exception) {
          transaction.Rollback();
        }
      }
    }

    public async Task<ThreadsIndexVm> GetThreadsIndexVmAsync(ClaimsPrincipal user, int topicId, int currentPage,
      int pageSize = 20) {

      var isAuthorizedForThreadCreate = await _authorizationService.IsAuthorizedForCreateThreadAsync(topicId, user);
      return await _db.Topic.Where(t => t.Id == topicId).Select(t => new ThreadsIndexVm {
        Pager = new Pager(t.Thread.Count, currentPage, pageSize),
        TopicId = t.Id,
        TopicText = t.ContentText,
        IsTopicLocked = t.LockedBy != null,
        Threads = t.Thread.OrderByDescending(th => th.CreatedOn).Skip((currentPage - 1) * pageSize).Take(pageSize).Select(th => new ThreadsIndexThreadVm {
          LatestPoster = th.Post.OrderByDescending(p => p.CreatedOn).Select(p => p.CreatedByNavigation.IdNavigation.UserName).FirstOrDefault(),
          LatestPostedOn = th.Post.OrderByDescending(p => p.CreatedOn).Select(p => p.CreatedOn).FirstOrDefault(),
          ThreadId = th.Id,
          CreatedOn = th.CreatedOn,
          CreatedBy = th.CreatedByNavigation.IdNavigation.UserName,
          ThreadText = th.ContentText,
          PostCount = th.Post.Count,
          LockedBy = th.LockedByNavigation.IdNavigation.UserName
        }).ToList(),
        IsAuthorizedForThreadCreate = isAuthorizedForThreadCreate
      }).FirstOrDefaultAsync();
    }

    public Task<ThreadEditVm> GetThreadEditVm(int id) {
      return _db.Thread.Where(t => t.Id == id).Select(t => new ThreadEditVm {
        ThreadId = t.Id,
        TopicId = t.Topic,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(ThreadEditVm threadEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FindAsync(threadEditVm.ThreadId);
      threadFromDb.ContentText = threadEditVm.ThreadText.Trim();
      threadFromDb.EditedBy = currentUserId;
      threadFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public Task<ThreadDeleteVm> GetThreadDeleteVm(int id) {
      return _db.Thread.Where(t => t.Id == id).Select(t => new ThreadDeleteVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        PostCount = t.Post.Count,
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(ThreadDeleteVm threadDeleteVm) {
      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadDeleteVm.ThreadId);
      using (var transaction = await _db.Database.BeginTransactionAsync()) {
        try {
          _db.Post.RemoveRange(_db.Post.Where(p => p.ThreadNavigation.Id == threadFromDb.Id));
          _db.Thread.Remove(threadFromDb);
          await _db.SaveChangesAsync();
          transaction.Commit();
        }
        catch (Exception) {
          transaction.Rollback();
        }
      }
    }

    public Task<ThreadLockVm> GetThreadLockVm(int id) {
      return _db.Thread.Where(t => t.Id == id).Select(t => new ThreadLockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        PostCount = t.Post.Count,
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }


    public async Task LockAsync(ThreadLockVm threadLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadLockVm.ThreadId);
      threadFromDb.LockedBy = currentUserId;
      threadFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public Task<ThreadUnlockVm> GetThreadUnlockVm(int id) {
      return _db.Thread.Where(t => t.Id == id).Select(t => new ThreadUnlockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = t.CreatedByNavigation.IdNavigation.UserName,
        PostCount = t.Post.Count,
        LockedBy = t.LockedByNavigation.IdNavigation.UserName,
        LockedOn = (DateTime) t.LockedOn,
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UnlockAsync(ThreadUnlockVm threadUnlockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadUnlockVm.ThreadId);
      threadFromDb.LockedBy = null;
      threadFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public async Task<ThreadOptionsVm> GetThreadOptionsVmAsync(int threadId, IPrincipal user, string returnUrl, string onRemoveReturnUrl) {
      var claimsPrincipalUser = user as ClaimsPrincipal;

      var isAuthorizedForThreadLock =
        await _authorizationService.IsAuthorizedForThreadLockAsync(threadId, claimsPrincipalUser);
      var isAuthorizedForThreadEdit =
        await _authorizationService.IsAuthorizedForThreadEditAsync(threadId, claimsPrincipalUser);
      var isAuthorizedForThreadDelete =
        await _authorizationService.IsAuthorizedForThreadDeleteAsync(threadId, claimsPrincipalUser);

      return await _db.Thread.Where(t => t.Id == threadId).Select(t => new ThreadOptionsVm {
        OnRemoveReturnUrl = onRemoveReturnUrl,
        ReturnUrl = returnUrl,
        TopicId = t.Topic,
        ThreadId = t.Id,
        LockedOn = t.LockedOn,
        IsAuthorizedForThreadLock = isAuthorizedForThreadLock,
        IsAuthorizedForThreadEdit = isAuthorizedForThreadEdit,
        IsAuthorizedForThreadDelete = isAuthorizedForThreadDelete
      }).FirstOrDefaultAsync();
    }

    public bool IsThreadLocked(int id) {
      return _db.Thread.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    public bool DoesThreadExist(int id) {
      return _db.Thread.Any(t => t.Id == id);
    }
  }
}