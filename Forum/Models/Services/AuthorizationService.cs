using System;
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

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForThreadEdit(int threadId, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && IsAuthorizedForThreadEdit(threadFromDb, user);
    }

    public bool IsAuthorizedForThreadEdit(Thread threadFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb?.TopicNavigation.LockedOn == null;
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // The Moderator follow the same rules as user but can delete other users threads (as long as they only contain post from thread
    // creator).
    // Admins have no restrictions.
    public async Task<bool> IsAuthorizedForThreadDelete(int threadId, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && IsAuthorizedForThreadDelete(threadFromDb, user);
    }

    public bool IsAuthorizedForThreadDelete(Thread threadFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin))
        return true;

      if (user.IsInRole(Roles.Moderator))
        return threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id)
          .All(p => p.CreatedBy == threadFromDb.CreatedBy);

      var userId = _userManager.GetUserId(user);
      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb?.TopicNavigation.LockedOn == null;
    }

    public bool IsAuthorizedForPostCreate(int threadId, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as the thread or post
    // is not locked and the post is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForPostEditAndDelete(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.Include(t => t.ThreadNavigation).FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && IsAuthorizedForPostEditAndDelete(postFromDb, user);
    }

    public bool IsAuthorizedForPostEditAndDelete(Post postFromDb, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      var userId = _userManager.GetUserId(user);
      return postFromDb.LockedOn == null && postFromDb.CreatedBy == userId &&
             postFromDb?.ThreadNavigation.LockedOn == null;
    }

    public bool IsAuthorizedForAccountAndPasswordEdit(string username, ClaimsPrincipal user) {
      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase) &&
             !user.IsInRole(Roles.Blocked);
    }

    public bool IsAuthorizedForAccountDetailsView(string username, ClaimsPrincipal user) {
      return user.IsInRole(Roles.Admin) ||
             string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public bool IsAuthorizedForProfileEdit(string username, ClaimsPrincipal user) {
      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase) &&
             !user.IsInRole(Roles.Blocked);
    }
  }
}