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

      // Includes TopicNavigation and Post to be used in IsUserAuthorized method
      threadsIndexVm.Threads.AddRange(_db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).Where(t => t.Topic == topicId).Select(t => new ThreadsIndexThreadVm() {
        ThreadId = t.Id,
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadText = t.ContentText,
        PostCount = t.Post.Count,
        IsUserAuthorized = IsUserAuthorized(t, user)
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

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsUserAuthorized(int id, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).FirstOrDefaultAsync(t => t.Id == id);
      if (threadFromDb == null)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId && threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) && threadFromDb?.TopicNavigation.LockedOn == null;
    }

    private bool IsUserAuthorized(Thread threadFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId && threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) && threadFromDb?.TopicNavigation.LockedOn == null;
    }

    public bool IsTopicLocked(int id) {
      return _db.Topic.Where(t => t.Id == id).Any(p => p.LockedBy != null);
    }
  }
}
