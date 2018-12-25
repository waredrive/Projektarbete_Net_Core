﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class AuthorizationService {
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationService(
      UserManager<IdentityUser> userManager, ForumDbContext db) {
      _userManager = userManager;
      _db = db;
    }

    public async Task<bool> IsAuthorizedForCreate(ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      return memberFromDb.BlockedBy == null;
    }

    public async Task<bool> IsAuthorizedForThreadCreateInTopic(int id, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      if (memberFromDb.BlockedBy != null)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Topic.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForThreadEdit(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadEdit(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadEdit(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb?.TopicNavigation.LockedOn == null && memberFromDb.BlockedBy == null;
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // The Moderator follow the same rules as user but can delete other users threads (as long as they only contain post from thread
    // creator).
    // Admins have no restrictions.
    public async Task<bool> IsAuthorizedForThreadDelete(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadDelete(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadDelete(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      if (user.IsInRole(Roles.Admin))
        return true;

      if (user.IsInRole(Roles.Moderator))
        return threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id)
          .All(p => p.CreatedBy == threadFromDb.CreatedBy);

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb?.TopicNavigation.LockedOn == null && memberFromDb.BlockedBy == null;
    }

    public async Task<bool> IsAuthorizedForPostCreateInThread(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      if (memberFromDb.BlockedBy != null)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as the thread or post
    // is not locked and the post is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForPostEditAndDelete(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.Include(t => t.ThreadNavigation).FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && await IsAuthorizedForPostEditAndDelete(postFromDb, user);
    }

    public async Task<bool> IsAuthorizedForPostEditAndDelete(Post postFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return postFromDb.LockedOn == null && postFromDb.CreatedBy == userId &&
             postFromDb?.ThreadNavigation.LockedOn == null && memberFromDb.BlockedBy == null;
    }

    public async Task<bool> IsAuthorizedForAccountAndProfileEdit(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated) return false;

      var identityFromDb = await _userManager.FindByNameAsync(user.Identity.Name);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityFromDb.Id);
      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase) &&
             memberFromDb.BlockedBy == null;
    }

    public bool IsAuthorizedForAccountDetailsView(string username, ClaimsPrincipal user) {
      return user.IsInRole(Roles.Admin) ||
             string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
    }
  }
}