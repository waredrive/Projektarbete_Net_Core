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

    public async Task<bool> IsAuthorizedForCreatePost(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return _db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy == null);
    }

    public async Task<bool> IsAuthorizedForCreateThread(int topicId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return _db.Topic.Where(t => t.Id == topicId).Any(t => t.LockedBy == null);
    }

    public async Task<bool> IsAuthorizedForCreateTopic(ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      return  user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForTopicEditBlockAndDelete(int topicId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicId);
      return topicFromDb != null && await IsAuthorizedForTopicEditBlockAndDelete(topicFromDb, user);
    }

    public async Task<bool> IsAuthorizedForTopicEditBlockAndDelete(Topic threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      return user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForThreadCreateInTopic(int id, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      if (await IsProfileBlocked(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Topic.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForThreadEdit(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadEdit(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadEdit(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await ProfileIsInRole(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) && threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

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
      if (!user.Identity.IsAuthenticated)
        return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadDelete(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadDelete(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await ProfileIsInRole(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) && threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin))
        return true;

      if (user.IsInRole(Roles.Moderator))
        return threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id)
          .All(p => p.CreatedBy == threadFromDb.CreatedBy);

      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb?.TopicNavigation.LockedOn == null;
    }

    public async Task<bool> IsAuthorizedForPostCreateInThread(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy != null);
    }

    public async Task<bool> IsAuthorizedForThreadLock(int threadId, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadLock(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadLock(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await ProfileIsInRole(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) && threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return false;
    }

    // The user is authorized as long as the thread or post
    // is not locked and the post is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForPostEditAndDelete(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.Include(t => t.ThreadNavigation).FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && await IsAuthorizedForPostEditAndDelete(postFromDb, user);
    }

    public async Task<bool> IsAuthorizedForPostEditAndDelete(Post postFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated && await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(postFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && postFromDb.CreatedBy != userId)
        return false;

      if (await ProfileIsInRole(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) && postFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return postFromDb.LockedOn == null && postFromDb.CreatedBy == userId &&
             postFromDb?.ThreadNavigation.LockedOn == null;
    }

    public async Task<bool> IsAuthorizedForPostLock(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && await IsAuthorizedForPostLock(postFromDb, user);
    }

    public async Task<bool> IsAuthorizedForPostLock(Post postFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated && await IsProfileBlocked(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(postFromDb.CreatedBy);

      if (await ProfileIsInRole(profileUser.UserName, Roles.Admin) && postFromDb.CreatedBy != userId)
        return false;

      if (await ProfileIsInRole(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) && postFromDb.CreatedBy != userId)
        return false;

      return user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator);
    }

    public async Task<bool> IsAuthorizedForAccountDetailsView(string username, ClaimsPrincipal user) {
      if (await IsProfileInternal(username))
        return false;

      if (await ProfileIsInRole(username, Roles.Admin) && !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      if (string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return true;

      if (await IsProfileBlocked(user.Identity.Name))
        return false;

      return user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForAccountAndProfileEdit(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileInternal(username) || await IsProfileBlocked(user.Identity.Name))
        return false;

      if (await ProfileIsInRole(username, Roles.Admin) && !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task<bool> IsAuthorizedForProfileDelete(string username, ClaimsPrincipal user) {
      if (await ProfileIsInRole(username, Roles.Admin) && !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase) || user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedProfileChangeRole(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileInternal(username) || await IsProfileBlocked(user.Identity.Name))
        return false;;

      return !await ProfileIsInRole(username, Roles.Admin) && user.IsInRole(Roles.Admin);
    }


    public async Task<bool> IsAuthorizedProfileBlock(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileInternal(username) || await IsProfileBlocked(user.Identity.Name))
        return false;

      if (await ProfileIsInRole(username, Roles.Admin))
        return false;

      if (await ProfileIsInRole(username, Roles.Moderator) && user.IsInRole(Roles.Moderator))
        return false;

      return user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator);
    }

    public async Task<bool> IsProfileInternal(string username) {
      var profileIdentityFromDb = await _userManager.FindByNameAsync(username);
      var profilMemberMemberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == profileIdentityFromDb.Id);

      return profilMemberMemberFromDb.IsInternal;
    }

    public async Task<bool> IsProfileBlocked(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var memberFromDb = await _db.Member.FirstOrDefaultAsync(m => m.Id == identityUser.Id);
      return memberFromDb.BlockedBy != null;
    }

    public async Task<bool> ProfileIsInRole(string username, string role) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var profileRoles = await _userManager.GetRolesAsync(identityUser);
      return profileRoles.Contains(role);
    }
  }
}