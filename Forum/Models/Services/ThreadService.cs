﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
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

    public async Task Add(ThreadCreateVm threadCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      using (var transaction = _db.Database.BeginTransaction()) {
        try {
          var thread = new Thread {
            Topic = threadCreateVm.TopicId,
            CreatedBy = currentUserId,
            ContentText = threadCreateVm.ThreadText,
            CreatedOn = DateTime.UtcNow
          };

          _db.Thread.Add(thread);

          var post = new Post {
            Thread = thread.Id,
            CreatedBy = currentUserId,
            ContentText = threadCreateVm.Post.PostText,
            CreatedOn = DateTime.UtcNow
          };

          _db.Post.Add(post);
          await _db.SaveChangesAsync();
          transaction.Commit();
        }
        catch (Exception) {
          transaction.Rollback();
        }
      }
    }

    public async Task<ThreadsIndexVm> GetThreadsIndexVm(int topicId, ClaimsPrincipal user) {
      var topicFromDb = await _db.Topic.Where(t => t.Id == topicId).FirstOrDefaultAsync();

      var threadsIndexVm = new ThreadsIndexVm {
        Topic = topicFromDb.ContentText,
        IsTopicLocked = topicFromDb.LockedBy != null,
        Threads = new List<ThreadsIndexThreadVm>(),
        IsAuthorizedForThreadCreate = await _authorizationService.IsAuthorizedForCreateThread(topicId, user)
      };

      //Included TopicNavigation  and Post to be used in IsAuthorizedForThreadEdit check within GetPostsIndexPostVmAsync method
      var threads = _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .Where(t => t.Topic == topicId);

      foreach (var thread in threads) threadsIndexVm.Threads.Add(await GetThreadsIndexThreadVmAsync(thread, user));

      return threadsIndexVm;
    }

    private async Task<ThreadsIndexThreadVm> GetThreadsIndexThreadVmAsync(Thread thread, ClaimsPrincipal user) {
      var isAuthorizedForThreadEdit = await _authorizationService.IsAuthorizedForThreadEdit(thread, user);
      var isAuthorizedForThreadDelete = await _authorizationService.IsAuthorizedForThreadDelete(thread, user);
      var isAuthorizedForThreadLock = await _authorizationService.IsAuthorizedForThreadLock(thread, user);
      var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);

      return new ThreadsIndexThreadVm {
        ThreadId = thread.Id,
        CreatedOn = thread.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadText = thread.ContentText,
        PostCount = thread.Post.Count,
        IsAuthorizedForThreadEdit = isAuthorizedForThreadEdit,
        IsAuthorizedForThreadDelete = isAuthorizedForThreadDelete,
        IsAuthorizedForThreadLock = isAuthorizedForThreadLock,
        LockedBy = thread.LockedBy != null ? _userManager.FindByIdAsync(thread.LockedBy).Result.UserName : null
      };
    }

    public async Task<ThreadEditVm> GetThreadEditVm(int id) {
      return await _db.Thread.Where(t => t.Id == id).Select(t => new ThreadEditVm {
        ThreadId = t.Id,
        TopicId = t.Topic,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Update(ThreadEditVm threadEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FindAsync(threadEditVm.ThreadId);
      threadFromDb.ContentText = threadEditVm.ThreadText;
      threadFromDb.EditedBy = currentUserId;
      threadFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<ThreadDeleteVm> GetThreadDeleteVm(int id) {
      return await _db.Thread.Where(t => t.Id == id).Select(t => new ThreadDeleteVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == t.Id),
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Remove(ThreadDeleteVm threadDeleteVm) {
      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadDeleteVm.ThreadId);
      using (var transaction = _db.Database.BeginTransaction()) {
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

    public async Task<ThreadLockVm> GetThreadLockVm(int id) {
      return await _db.Thread.Where(t => t.Id == id).Select(t => new ThreadLockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.Thread == t.Id),
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }


    public async Task Lock(ThreadLockVm threadLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadLockVm.ThreadId);
      threadFromDb.LockedBy = currentUserId;
      threadFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<ThreadUnlockVm> GetThreadUnlockVm(int id) {
      return await _db.Thread.Where(t => t.Id == id).Select(t => new ThreadUnlockVm {
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.Thread == t.Id),
        LockedBy = _userManager.FindByIdAsync(t.LockedBy).Result.UserName,
        LockedOn = (DateTime) t.LockedOn,
        ThreadId = t.Id,
        ThreadText = t.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Unlock(ThreadUnlockVm threadUnlockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadUnlockVm.ThreadId);
      threadFromDb.LockedBy = null;
      threadFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public bool IsThreadLocked(int id) {
      return _db.Thread.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    public bool DoesThreadExist(int id) {
      return _db.Thread.Any(t => t.Id == id);
    }
  }
}