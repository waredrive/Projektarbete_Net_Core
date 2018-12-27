using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ForumManagementViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ForumManagementService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly IHostingEnvironment _env;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ForumManagementService(
      UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,
      RoleManager<IdentityRole> roleManager, ForumDbContext db, AuthorizationService authorizationService,
      IHostingEnvironment env) {
      _userManager = userManager;
      _signInManager = signInManager;
      _roleManager = roleManager;
      _db = db;
      _authorizationService = authorizationService;
      _env = env;
    }

    public async Task<ForumManagementIndexVm> GetForumManagementIndexVm(ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
      var allBlockedMembers = await _db.Member.Where(m => m.BlockedBy != null).ToListAsync();
      var allLockedTopics = await _db.Topic.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedThreads = await _db.Thread.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedPosts = await _db.Post.Where(m => m.LockedBy != null).ToListAsync();

      var forumManagementIndexVm = new ForumManagementIndexVm {
        LatestBlockedUsers = new List<ForumManagementIndexBlockedUserVm>(),
        LatestLockedPosts = new List<ForumManagementIndexLockedPostVm>(),
        LatestLockedThreads = new List<ForumManagementIndexLockedThreadVm>(),
        LatestLockedTopics = new List<ForumManagementIndexLockedTopicVm>(),
        BlockedMembersCount = allBlockedMembers.Count(),
        LockedTopicsCount = allLockedTopics.Count(),
        LockedThreadsCount = allLockedThreads.Count(),
        LockedPostsCount = allLockedPosts.Count(),
        BlockedByUserMembersCount = allBlockedMembers.Count(m => m.BlockedBy == identityUser.Id),
        LockedByUserTopicsCount = allLockedTopics.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserThreadsCount = allLockedThreads.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserPostsCount = allLockedPosts.Count(m => m.LockedBy == identityUser.Id)
      };


      var topLockedTopics = allLockedTopics.OrderByDescending(t => t.LockedOn).Take(10);

      foreach (var lockedTopic in topLockedTopics)
        forumManagementIndexVm.LatestLockedTopics.Add(await GetForumManagementIndexLockedTopicVm(lockedTopic, user));

      var topLockedThreads = allLockedThreads.OrderByDescending(t => t.LockedOn).Take(10);

      foreach (var lockedThread in topLockedThreads)
        forumManagementIndexVm.LatestLockedThreads.Add(await GetForumManagementIndexLockedThreadVm(lockedThread, user));

      var topLockedPosts = allLockedPosts.OrderByDescending(p => p.LockedOn).Take(10);

      foreach (var lockedPost in topLockedPosts)
        forumManagementIndexVm.LatestLockedPosts.Add(await GetForumManagementIndexLockedPostVm(lockedPost, user));

      var topBlockedMembers = allBlockedMembers.OrderByDescending(p => p.BlockedOn).Take(10);

      foreach (var blockedMember in topBlockedMembers)
        forumManagementIndexVm.LatestBlockedUsers.Add(await GetForumManagementIndexBlockedUserVm(blockedMember, user));

      return forumManagementIndexVm;
    }

    private async Task<ForumManagementIndexLockedTopicVm> GetForumManagementIndexLockedTopicVm(Topic topic,
      ClaimsPrincipal user) {
      var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);
      var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);

      return new ForumManagementIndexLockedTopicVm {
        TopicId = topic.Id,
        CreatedOn = topic.CreatedOn,
        LockedBy = lockedBy.UserName,
        LockedOn = topic.LockedOn,
        CreatedBy = createdBy.UserName,
        TopicText = topic.ContentText,
        IsAuthorizedForTopicEditLockAndDelete =
          await _authorizationService.IsAuthorizedForTopicEditLockAndDelete(topic, user)
      };
    }

    private async Task<ForumManagementIndexLockedThreadVm> GetForumManagementIndexLockedThreadVm(Thread thread,
      ClaimsPrincipal user) {
      var lockedBy = await _userManager.FindByIdAsync(thread.LockedBy);
      var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);

      return new ForumManagementIndexLockedThreadVm {
        TopicId = thread.Topic,
        ThreadId = thread.Id,
        CreatedOn = thread.CreatedOn,
        LockedBy = lockedBy.UserName,
        LockedOn = thread.LockedOn,
        CreatedBy = createdBy.UserName,
        ThreadText = thread.ContentText,
        IsAuthorizedForThreadEdit =
          await _authorizationService.IsAuthorizedForThreadEdit(thread, user),
        IsAuthorizedForThreadLock = await _authorizationService.IsAuthorizedForThreadLock(thread, user),
        IsAuthorizedForThreadDelete = await _authorizationService.IsAuthorizedForThreadDelete(thread, user)
      };
    }

    private async Task<ForumManagementIndexLockedPostVm> GetForumManagementIndexLockedPostVm(Post post,
      ClaimsPrincipal user) {
      var lockedBy = await _userManager.FindByIdAsync(post.LockedBy);
      var createdBy = await _userManager.FindByIdAsync(post.CreatedBy);

      return new ForumManagementIndexLockedPostVm {
        ThreadId = post.Thread,
        PostId = post.Id,
        CreatedOn = post.CreatedOn,
        LockedBy = lockedBy.UserName,
        LockedOn = post.LockedOn,
        CreatedBy = createdBy.UserName,
        PostText = post.ContentText,
        IsAuthorizedForPostEditAndDelete = await _authorizationService.IsAuthorizedForPostEditAndDelete(post, user),
        IsAuthorizedForPostLock = await _authorizationService.IsAuthorizedForPostLock(post, user)
      };
    }

    private async Task<ForumManagementIndexBlockedUserVm> GetForumManagementIndexBlockedUserVm(Member member,
      ClaimsPrincipal user) {
      var blockedBy = await _userManager.FindByIdAsync(member.BlockedBy);
      var identityUser = await _userManager.FindByIdAsync(member.Id);
      var roles = await _userManager.GetRolesAsync(identityUser);

      return new ForumManagementIndexBlockedUserVm {
        MemberId = member.Id,
        CreatedOn = member.CreatedOn,
        BlockedBy = blockedBy.UserName,
        BlockedOn = member.BlockedOn,
        BlockEnd = member.BlockedEnd,
        Username = identityUser.UserName,
        Roles = roles.ToArray(),
        IsAuthorizedForProfileEdit =
          await _authorizationService.IsAuthorizedForAccountAndProfileEdit(identityUser.UserName, user),
        IsAuthorizedForProfileDelete =
          await _authorizationService.IsAuthorizedForProfileDelete(identityUser.UserName, user),
        IsAuthorizedProfileBlock = await _authorizationService.IsAuthorizedProfileBlock(identityUser.UserName, user),
        IsAuthorizedProfileChangeRole =
          await _authorizationService.IsAuthorizedProfileChangeRole(identityUser.UserName, user)
      };
    }
  }
}