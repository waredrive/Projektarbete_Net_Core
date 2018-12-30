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
    private readonly SharedService _sharedService;
    private readonly UserManager<IdentityUser> _userManager;

    public ThreadService(ForumDbContext db, AuthorizationService authorizationService,
      UserManager<IdentityUser> userManager, SharedService sharedService) {
      _db = db;
      _authorizationService = authorizationService;
      _userManager = userManager;
      _sharedService = sharedService;
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
      var topicFromDb = await _db.Topic.Where(t => t.Id == topicId).FirstOrDefaultAsync();

      var threadsIndexVm = new ThreadsIndexVm {
        Pager = await GetPaginationVmForThreads(topicId, currentPage, pageSize),
        TopicId = topicFromDb.Id,
        TopicText = topicFromDb.ContentText,
        IsTopicLocked = topicFromDb.LockedBy != null,
        Threads = new List<ThreadsIndexThreadVm>(),
        IsAuthorizedForThreadCreate = await _authorizationService.IsAuthorizedForCreateThreadAsync(topicId, user)
      };

      //Included TopicNavigation  and Post to be used in IsAuthorizedForThreadEdit check within GetPostsIndexPostVmAsync method
      var threads = _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post).OrderByDescending(t => t.CreatedOn)
        .Where(t => t.Topic == topicId).Skip((currentPage - 1) * pageSize).Take(pageSize);

      foreach (var thread in threads) threadsIndexVm.Threads.Add(await GetThreadsIndexThreadVmAsync(thread, user));

      return threadsIndexVm;
    }

    private async Task<Pager> GetPaginationVmForThreads(int topicId, int currentPage, int pageSize) {
      var totalItems = await _db.Thread.Where(t => t.Topic == topicId).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }

    private async Task<ThreadsIndexThreadVm> GetThreadsIndexThreadVmAsync(Thread thread, ClaimsPrincipal user) {
      var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);
      var latestPostInThread = thread.Post.OrderByDescending(p => p.CreatedOn).FirstOrDefault();
      var latestPoster = latestPostInThread != null
        ? await _userManager.FindByIdAsync(latestPostInThread.CreatedBy)
        : null;

      return new ThreadsIndexThreadVm {
        LatestPoster = latestPoster?.UserName,
        LatestPosterProfileImage = await _sharedService.GetProfileImageStringByUsernameAsync(latestPoster?.UserName),
        LatestPostedOn = latestPostInThread?.CreatedOn,
        ThreadId = thread.Id,
        CreatedOn = thread.CreatedOn,
        CreatedBy = createdBy.UserName,
        ThreadText = thread.ContentText,
        PostCount = thread.Post.Count,
        LockedBy = thread.LockedBy != null ? (await _userManager.FindByIdAsync(thread.LockedBy)).UserName : null
      };
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
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.ThreadNavigation.Topic == t.Id),
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
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.Thread == t.Id),
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
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        PostCount = _db.Post.Count(p => p.Thread == t.Id),
        LockedBy = _userManager.FindByIdAsync(t.LockedBy).Result.UserName,
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

    public async Task<ThreadOptionsVm> GetThreadOptionsVmAsync(int threadId, IPrincipal user, string returnUrl) {
      var claimsPrincipalUser = user as ClaimsPrincipal;
      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadId);

      var isAuthorizedForThreadLock =
        await _authorizationService.IsAuthorizedForThreadLockAsync(threadId, claimsPrincipalUser);
      var isAuthorizedForThreadEdit =
        await _authorizationService.IsAuthorizedForThreadEditAsync(threadId, claimsPrincipalUser);
      var isAuthorizedForThreadDelete =
        await _authorizationService.IsAuthorizedForThreadDeleteAsync(threadId, claimsPrincipalUser);

      return new ThreadOptionsVm {
        ReturnUrl = returnUrl,
        TopicId = threadFromDb.Topic,
        ThreadId = threadId,
        LockedOn = threadFromDb.LockedOn,
        IsAuthorizedForThreadLock = isAuthorizedForThreadLock,
        IsAuthorizedForThreadEdit = isAuthorizedForThreadEdit,
        IsAuthorizedForThreadDelete = isAuthorizedForThreadDelete
      };
    }

    public bool IsThreadLocked(int id) {
      return _db.Thread.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    public bool DoesThreadExist(int id) {
      return _db.Thread.Any(t => t.Id == id);
    }
  }
}