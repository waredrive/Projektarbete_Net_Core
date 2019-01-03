using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.Identity;
using Forum.Models.Pagination;
using Forum.Models.ViewModels.ComponentViewModels.PostOptionsViewModels;
using Forum.Models.ViewModels.PostViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class PostService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public PostService(ForumDbContext db, AuthorizationService authorizationService,
      UserManager<IdentityUser> userManager) {
      _db = db;
      _authorizationService = authorizationService;
      _userManager = userManager;
    }

    public async Task AddAsync(PostCreateVm postCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var post = new Post {
        Thread = postCreateVm.ThreadId,
        CreatedBy = currentUserId,
        ContentText = postCreateVm.PostText.Trim(),
        CreatedOn = DateTime.UtcNow
      };

      _db.Post.Add(post);
      await _db.SaveChangesAsync();
    }

    public async Task<PostsIndexVm> GetPostsIndexVmAsync(ClaimsPrincipal user, int threadId, int currentPage,
      int? postId = null, int pageSize = 15) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Where(t => t.Id == threadId)
        .FirstOrDefaultAsync();

      if (postId != null) currentPage = await FindPageWithPostAsync((int) postId, threadId, pageSize);

      var postsIndexVm = new PostsIndexVm {
        Pager = await GetPaginationVmForPosts(threadId, currentPage, pageSize),
        TopicText = threadFromDb.TopicNavigation.ContentText,
        TopicId = threadFromDb.Topic,
        ThreadId = threadFromDb.Id,
        ThreadText = threadFromDb.ContentText,
        Posts = new List<PostsIndexPostVm>(),
        IsThreadLocked = threadFromDb.LockedBy != null,
        IsAuthorizedForPostCreate = await _authorizationService.IsAuthorizedForCreatePostAsync(threadId, user)
      };

      var posts = _db.Post.OrderBy(t => t.CreatedOn)
        .Where(p => p.Thread == threadId).Skip((currentPage - 1) * pageSize).Take(pageSize);

      foreach (var post in posts)
        postsIndexVm.Posts.Add(await GetPostsIndexPostVmAsync(post));
      return postsIndexVm;
    }

    private async Task<int> FindPageWithPostAsync(int postId, int threadId, int pageSize, int currentPage = 1) {
      var page = currentPage;

      while (!await _db.Post.OrderBy(t => t.CreatedOn)
        .Where(p => p.Thread == threadId).Skip((page - 1) * pageSize).Take(pageSize).AnyAsync(p => p.Id == postId))
        await FindPageWithPostAsync(postId, threadId, pageSize, ++page);
      return page;
    }

    private async Task<Pager> GetPaginationVmForPosts(int threadId, int currentPage, int pageSize) {
      var totalItems = await _db.Post.Where(t => t.Thread == threadId).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }

    private async Task<PostsIndexPostVm> GetPostsIndexPostVmAsync(Post post) {
      var createdBy = await _userManager.FindByIdAsync(post.CreatedBy);
      var lockedBy = await _userManager.FindByIdAsync(post.LockedBy);
      var editedBy = await _userManager.FindByIdAsync(post.EditedBy);

      return new PostsIndexPostVm {
        PostId = post.Id,
        CreatedOn = post.CreatedOn,
        CreatedBy = createdBy.UserName,
        CreatorsTotalposts = await _db.Post.CountAsync(p => p.CreatedBy == post.CreatedBy),
        CreatorsRoles = (await _userManager.GetRolesAsync(createdBy)).ToArray(),
        PostText = post.ContentText,
        LockedBy = lockedBy?.UserName,
        LockedOn = post.LockedOn,
        EditedBy = editedBy?.UserName,
        EditedOn = post.EditedOn
      };
    }

    public Task<PostEditVm> GetPostEditVm(int id) {
      return _db.Post.Where(p => p.Id == id).Select(p => new PostEditVm {
        ThreadId = p.Thread,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(PostEditVm postEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var postFromDb = await _db.Post.FindAsync(postEditVm.PostId);
      postFromDb.ContentText = postEditVm.PostText.Trim();
      postFromDb.EditedBy = currentUserId;
      postFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public Task<PostDeleteVm> GetPostDeleteVm(int id) {
      return _db.Post.Where(p => p.Id == id).Select(p => new PostDeleteVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task RemoveAsync(PostDeleteVm postDeleteVm) {
      var postFromDb = await _db.Post.FirstOrDefaultAsync(t => t.Id == postDeleteVm.PostId);
      _db.Post.Remove(postFromDb);
      await _db.SaveChangesAsync();
    }

    public Task<PostLockVm> GetPostLockVm(int id) {
      return _db.Post.Where(p => p.Id == id).Select(p => new PostLockVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task LockAsync(PostLockVm postLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var postFromDb = await _db.Post.FirstOrDefaultAsync(p => p.Id == postLockVm.PostId);
      postFromDb.LockedBy = currentUserId;
      postFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public Task<PostUnlockVm> GetPostUnlockVm(int id) {
      return _db.Post.Where(p => p.Id == id).Select(p => new PostUnlockVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        LockedBy = _userManager.FindByIdAsync(p.LockedBy).Result.UserName,
        LockedOn = (DateTime) p.LockedOn,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task UnlockAsync(PostUnlockVm postUnlockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var postFromDb = await _db.Post.FirstOrDefaultAsync(p => p.Id == postUnlockVm.PostId);
      postFromDb.LockedBy = null;
      postFromDb.LockedOn = null;

      await _db.SaveChangesAsync();
    }

    public bool IsPostLocked(int id) {
      return _db.Post.Where(p => p.Id == id).Any(p => p.LockedBy != null);
    }

    public bool DoesPostExist(int id) {
      return _db.Post.Any(p => p.Id == id);
    }

    public async Task<PostOptionsVm> GetPostOptionsVmAsync(int postId, IPrincipal user, string returnUrl) {
      var claimsPrincipalUser = user as ClaimsPrincipal;
      var postFromDb = await _db.Post.FirstOrDefaultAsync(p => p.Id == postId);

      var isAuthorizedForPostEditAndDelete =
        await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(postFromDb, claimsPrincipalUser);
      var isAuthorizedForPostLock =
        await _authorizationService.IsAuthorizedForPostLockAsync(postFromDb, claimsPrincipalUser);

      return new PostOptionsVm {
        ReturnUrl = returnUrl,
        ThreadId = postFromDb.Thread,
        PostId = postFromDb.Id,
        LockedOn = postFromDb.LockedOn,
        IsAuthorizedForPostEditAndDelete = isAuthorizedForPostEditAndDelete,
        IsAuthorizedForPostLock = isAuthorizedForPostLock
      };
    }
  }
}