using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ThreadViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ThreadService {
    private readonly ForumDbContext _db;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ThreadService(ForumDbContext db, UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager) {
      _db = db;
      _userManager = userManager;
      _signInManager = signInManager;
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
        } catch (Exception) {
          transaction.Rollback();
        }
      }
    }

    public async Task<ThreadsIndexVm> GetThreadsIndexVm(int topicId, ClaimsPrincipal user) {
      var topicFromDb = await _db.Topic.Where(t => t.Id == topicId).FirstOrDefaultAsync();

      var threadsIndexVm = new ThreadsIndexVm {
        Topic = topicFromDb.ContentText,
        IsTopicLocked = topicFromDb.LockedBy != null,
        Threads = new List<ThreadsIndexThreadVm>()
      };

      // Includes TopicNavigation and Post to be used in IsAuthorizedForThreadEdit method
      threadsIndexVm.Threads.AddRange(_db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).Where(t => t.Topic == topicId).Select(t => new ThreadsIndexThreadVm() {
        ThreadId = t.Id,
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadText = t.ContentText,
        PostCount = t.Post.Count,
        IsAuthorizedForThreadEdit = IsAuthorizedForThreadEdit(t, user),
        IsAuthorizedForThreadDelete = IsAuthorizedForThreadDelete(t, user),
        LockedBy = t.LockedBy != null ? _userManager.FindByIdAsync(t.LockedBy).Result.UserName : null
      }));

      return threadsIndexVm;
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
        ThreadText= t.ContentText,
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
        } catch (Exception) {
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
        LockedOn = (DateTime)t.LockedOn,
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

    public bool IsAuthorizedForThreadCreate(int id, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Topic.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForThreadEdit(int id, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).FirstOrDefaultAsync(t => t.Id == id);
      return threadFromDb != null && IsAuthorizedForThreadEdit(threadFromDb, user);
    }

    private bool IsAuthorizedForThreadEdit(Thread threadFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId && threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) && threadFromDb?.TopicNavigation.LockedOn == null;
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // The Moderator follow the same rules as user but can delete other users threads (as long as they only contain post from thread
    // creator).
    // Admins have no restrictions.
    public async Task<bool> IsAuthorizedForThreadDelete(int id, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).FirstOrDefaultAsync(t => t.Id == id);
      return threadFromDb != null && IsAuthorizedForThreadDelete(threadFromDb, user);
    }

    private bool IsAuthorizedForThreadDelete(Thread threadFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin))
        return true;

      if (user.IsInRole(Roles.Moderator)) {
        return threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == threadFromDb.CreatedBy);
      }

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId && threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) && threadFromDb?.TopicNavigation.LockedOn == null;
    }

    public bool IsThreadLocked(int id) {
      return _db.Thread.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

  }
}
