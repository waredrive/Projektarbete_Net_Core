using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class AuthorizationService {
    private readonly ForumDbContext _db;
    private readonly SharedService _sharedService;
    private readonly UserManager<IdentityUser> _userManager;
    private const double MinutesToAllowEditAndDelete = 15;

    public AuthorizationService(
      UserManager<IdentityUser> userManager, ForumDbContext db, SharedService sharedService) {
      _userManager = userManager;
      _db = db;
      _sharedService = sharedService;
    }

    public async Task<bool> IsAuthorizedForCreatePostAsync(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return _db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy == null);
    }

    public async Task<bool> IsAuthorizedForCreateThreadAsync(int topicId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return _db.Topic.Where(t => t.Id == topicId).Any(t => t.LockedBy == null);
    }

    public async Task<bool> IsAuthorizedForCreateTopicAsync(ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      return user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForTopicEditLockAndDeleteAsync(int topicId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      var topicFromDb = await _db.Topic.FirstOrDefaultAsync(t => t.Id == topicId);
      return topicFromDb != null && await IsAuthorizedForTopicEditLockAndDeleteAsync(topicFromDb, user);
    }

    public async Task<bool> IsAuthorizedForTopicEditLockAndDeleteAsync(Topic threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      return user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForThreadCreateInTopicAsync(int id, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      if (await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Topic.Where(t => t.Id == id).Any(t => t.LockedBy != null);
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForThreadEditAsync(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadEditAsync(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadEditAsync(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) &&
          threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb.TopicNavigation.LockedOn == null && threadFromDb.CreatedOn >= DateTime.UtcNow.AddMinutes(-MinutesToAllowEditAndDelete);
    }

    // The user is authorized as long as he is the only one that posted on the thread,
    // the thread or topic is not locked and the thread is created by the user.
    // The Moderator follow the same rules as user but can delete other users threads (as long as they only contain post from thread
    // creator).
    // Admins have no restrictions.
    public async Task<bool> IsAuthorizedForThreadDeleteAsync(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      var threadFromDb = await _db.Thread.Include(t => t.TopicNavigation).Include(t => t.Post)
        .FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadDeleteAsync(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadDeleteAsync(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) &&
          threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin))
        return true;

      if (user.IsInRole(Roles.Moderator))
        return threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id)
          .All(p => p.CreatedBy == threadFromDb.CreatedBy);

      return threadFromDb.LockedOn == null && threadFromDb.CreatedBy == userId &&
             threadFromDb.Post.Where(p => p.Thread == threadFromDb.Id).All(p => p.CreatedBy == userId) &&
             threadFromDb.TopicNavigation.LockedOn == null && threadFromDb.CreatedOn >= DateTime.UtcNow.AddMinutes(-MinutesToAllowEditAndDelete);
    }

    public async Task<bool> IsAuthorizedForPostCreateInThreadAsync(int threadId, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return !_db.Thread.Where(t => t.Id == threadId).Any(t => t.LockedBy != null);
    }

    public async Task<bool> IsAuthorizedForThreadLockAsync(int threadId, ClaimsPrincipal user) {
      var threadFromDb = await _db.Thread.FirstOrDefaultAsync(t => t.Id == threadId);
      return threadFromDb != null && await IsAuthorizedForThreadLockAsync(threadFromDb, user);
    }

    public async Task<bool> IsAuthorizedForThreadLockAsync(Thread threadFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(threadFromDb.CreatedBy);

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && threadFromDb.CreatedBy != userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) &&
          threadFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return false;
    }

    // The user is authorized as long as the thread or post
    // is not locked and the post is created by the user.
    // Admins and Moderators have no restrictions.
    public async Task<bool> IsAuthorizedForPostEditAndDeleteAsync(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.Include(t => t.ThreadNavigation).FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && await IsAuthorizedForPostEditAndDeleteAsync(postFromDb, user);
    }

    public async Task<bool> IsAuthorizedForPostEditAndDeleteAsync(Post postFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(postFromDb.CreatedBy);

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && postFromDb.CreatedBy != userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) &&
          postFromDb.CreatedBy != userId)
        return false;

      if (user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator))
        return true;

      return postFromDb.LockedOn == null && postFromDb.CreatedBy == userId &&
             postFromDb.ThreadNavigation.LockedOn == null && postFromDb.CreatedOn >= DateTime.UtcNow.AddMinutes(-MinutesToAllowEditAndDelete);
    }

    public async Task<bool> IsAuthorizedForPostLockAsync(int postId, ClaimsPrincipal user) {
      var postFromDb = await _db.Post.FirstOrDefaultAsync(t => t.Id == postId);
      return postFromDb != null && await IsAuthorizedForPostLockAsync(postFromDb, user);
    }

    public async Task<bool> IsAuthorizedForPostLockAsync(Post postFromDb, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      var userId = _userManager.GetUserId(user);
      var profileUser = await _userManager.FindByIdAsync(postFromDb.CreatedBy);

      if (postFromDb.CreatedBy == userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Admin) && postFromDb.CreatedBy != userId)
        return false;

      if (await IsProfileInRoleAsync(profileUser.UserName, Roles.Moderator) && user.IsInRole(Roles.Moderator) &&
          postFromDb.CreatedBy != userId)
        return false;

      return user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator);
    }

    public async Task<bool> IsAuthorizedForAccountDetailsViewAsync(string username, ClaimsPrincipal user) {
      if (_sharedService.IsDeletedMember(username))
        return false;

      if (await IsProfileInRoleAsync(username, Roles.Admin) &&
          !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      if (string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return true;

      if (await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      return user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedForAccountAndProfileEditAsync(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || _sharedService.IsDeletedMember(username) ||
          await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (await IsProfileInRoleAsync(username, Roles.Admin) &&
          !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task<bool> IsAuthorizedForProfileDeleteAsync(string username, ClaimsPrincipal user) {
      if (!_sharedService.DoesUserAccountExist(username))
        return false;

      if (_sharedService.IsDeletedMember(username))
        return false;

      if (await IsProfileInRoleAsync(username, Roles.Admin) &&
          !string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase))
        return false;

      return string.Equals(username, user.Identity.Name, StringComparison.CurrentCultureIgnoreCase) ||
             user.IsInRole(Roles.Admin);
    }

    public async Task<bool> IsAuthorizedProfileChangeRoleAsync(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || _sharedService.IsDeletedMember(username) ||
          await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      return !await IsProfileInRoleAsync(username, Roles.Admin) && user.IsInRole(Roles.Admin);
    }


    public async Task<bool> IsAuthorizedProfileBlockAsync(string username, ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated || _sharedService.IsDeletedMember(username) ||
          await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      if (await IsProfileInRoleAsync(username, Roles.Admin))
        return false;

      if (await IsProfileInRoleAsync(username, Roles.Moderator) && user.IsInRole(Roles.Moderator))
        return false;

      return user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Moderator);
    }

    public Task<bool> IsProfileBlockedAsync(string username) {
      return _db.Member.Where(m => m.IdNavigation.UserName == username).AnyAsync(m => m.BlockedBy != null);
    }

    private async Task<bool> IsProfileInRoleAsync(string username, string role) {
      var identityUser = await _userManager.FindByNameAsync(username);
      var profileRoles = await _userManager.GetRolesAsync(identityUser);
      return profileRoles.Contains(role);
    }

    public async Task<bool> IsAuthorizedForForumManagementAsync(ClaimsPrincipal user) {
      if (!user.Identity.IsAuthenticated)
        return false;

      if (await IsProfileBlockedAsync(user.Identity.Name))
        return false;

      return user.IsInRole(Roles.Admin);
    }
  }
}