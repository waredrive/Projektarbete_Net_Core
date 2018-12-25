using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
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

    public async Task Add(PostCreateVm postCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var post = new Post {
        Thread = postCreateVm.ThreadId,
        CreatedBy = currentUserId,
        ContentText = postCreateVm.PostText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Post.Add(post);
      await _db.SaveChangesAsync();
    }

    public async Task<PostsIndexVm> GetPostsIndexVm(int threadId, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Where(t => t.Id == threadId).FirstOrDefaultAsync();

      var postsIndexVm = new PostsIndexVm {
        Thread = threadFromDb.ContentText,
        Posts = new List<PostsIndexPostVm>(),
        IsThreadLocked = threadFromDb.LockedBy != null,
        IsAuthorizedForPostCreate =await _authorizationService.IsAuthorizedForCreate(user)
      };

      //Included Threadnavigation to be used in authorization check within GetPostsIndexPostVmAsync method
      var posts = _db.Post.Include(p => p.ThreadNavigation).Where(p => p.Thread == threadId);

      foreach (var post in posts) {
        postsIndexVm.Posts.Add(await GetPostsIndexPostVmAsync(post, user));
      }
      return postsIndexVm;
    }

    private async Task<PostsIndexPostVm> GetPostsIndexPostVmAsync(Post post, ClaimsPrincipal user) {
      var isAuthorizedForPostEditAndDelete = await _authorizationService.IsAuthorizedForPostEditAndDelete(post, user);

      return new PostsIndexPostVm {
        PostId = post.Id,
        CreatedOn = post.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(post.CreatedBy).Result.UserName,
        PostText = post.ContentText,
        IsAuthorizedForPostEditAndDelete = isAuthorizedForPostEditAndDelete,
        LockedBy = post.LockedBy != null ? _userManager.FindByIdAsync(post.LockedBy).Result.UserName : null
      };
    }

    public async Task<PostEditVm> GetPostEditVm(int id) {
      return await _db.Post.Where(p => p.Id == id).Select(p => new PostEditVm {
        ThreadId = p.Thread,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Update(PostEditVm postEditVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var postFromDb = await _db.Post.FindAsync(postEditVm.PostId);
      postFromDb.ContentText = postEditVm.PostText;
      postFromDb.EditedBy = currentUserId;
      postFromDb.EditedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<PostDeleteVm> GetPostDeleteVm(int id) {
      return await _db.Post.Where(p => p.Id == id).Select(p => new PostDeleteVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Remove(PostDeleteVm postDeleteVm) {
      var postFromDb = await _db.Post.FirstOrDefaultAsync(t => t.Id == postDeleteVm.PostId);
      _db.Post.Remove(postFromDb);
      await _db.SaveChangesAsync();
    }

    public async Task<PostLockVm> GetPostLockVm(int id) {
      return await _db.Post.Where(p => p.Id == id).Select(p => new PostLockVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Lock(PostLockVm postLockVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var postFromDb = await _db.Post.FirstOrDefaultAsync(p => p.Id == postLockVm.PostId);
      postFromDb.LockedBy = currentUserId;
      postFromDb.LockedOn = DateTime.UtcNow;

      await _db.SaveChangesAsync();
    }

    public async Task<PostUnlockVm> GetPostUnlockVm(int id) {
      return await _db.Post.Where(p => p.Id == id).Select(p => new PostUnlockVm {
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        LockedBy = _userManager.FindByIdAsync(p.LockedBy).Result.UserName,
        LockedOn = (DateTime) p.LockedOn,
        PostId = p.Id,
        PostText = p.ContentText
      }).FirstOrDefaultAsync();
    }

    public async Task Unlock(PostUnlockVm postUnlockVm, ClaimsPrincipal user) {
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
  }
}