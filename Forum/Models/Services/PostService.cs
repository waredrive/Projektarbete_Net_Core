using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.PostViewModel;
using Forum.Models.ViewModels.PostViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class PostService {
    private readonly ForumDbContext _db;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public PostService(ForumDbContext db, UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager) {
      _db = db;
      _userManager = userManager;
      _signInManager = signInManager;
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
        IsThreadLocked = threadFromDb.LockedBy != null
      };

      //Included Threadnavigation to be used in IsAuthorizedForPostEditAndDelete method
      postsIndexVm.Posts.AddRange(_db.Post.Include(p => p.ThreadNavigation).Where(p => p.Thread == threadId).Select(p => new PostsIndexPostVm {
        PostId = p.Id,
        CreatedOn = p.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
        PostText = p.ContentText,
        IsAuthorizedForPostEditAndDelete = IsAuthorizedForPostEditAndDelete(p, user),
        LockedBy = p.LockedBy != null ? _userManager.FindByIdAsync(p.LockedBy).Result.UserName : null
      }));

      return postsIndexVm;
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
        PostText = p.ContentText,
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
        PostText = p.ContentText,
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
        LockedOn = (DateTime)p.LockedOn,
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

    public bool IsAuthorizedForPostCreate(int id, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Thread.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as the thread or post
    // is not locked and the post is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForPostEditAndDelete(int id, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.Include(t => t.ThreadNavigation).FirstOrDefaultAsync(t => t.Id == id);
      return postFromDb != null && IsAuthorizedForPostEditAndDelete(postFromDb, user);
    }

    private bool IsAuthorizedForPostEditAndDelete(Post postFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return postFromDb.LockedOn == null && postFromDb.CreatedBy == userId && postFromDb?.ThreadNavigation.LockedOn == null;
    }

    public bool IsPostLocked(int id) {
      return _db.Post.Where(p => p.Id == id).Any(p => p.LockedBy != null);
    }
  }
}
