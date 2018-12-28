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
      var allBlockedMembers = await _db.Member.Where(m => m.BlockedBy != null).ToListAsync();
      var allLockedTopics = await _db.Topic.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedThreads = await _db.Thread.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedPosts = await _db.Post.Where(m => m.LockedBy != null).ToListAsync();

      var forumManagementIndexVm = new ForumManagementIndexVm {
        LatestBlockedMembers = new List<ForumManagementBlockedMemberVm>(),
        LatestLockedPosts = new List<ForumManagementLockedPostVm>(),
        LatestLockedThreads = new List<ForumManagementLockedThreadVm>(),
        LatestLockedTopics = new List<ForumManagementLockedTopicVm>(),
        Statistics = await GetForumManagementStatisticsVm(allLockedTopics, allLockedThreads, allLockedPosts,
          allBlockedMembers, user)
      };

      forumManagementIndexVm.LatestLockedTopics =
        await GetForumManagementLockedTopicsVm(allLockedTopics.OrderByDescending(t => t.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestLockedThreads =
        await GetForumManagementLockedThreadsVm(allLockedThreads.OrderByDescending(t => t.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestLockedPosts =
        await GetForumManagementLockedPostsVm(allLockedPosts.OrderByDescending(p => p.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestBlockedMembers =
        await GetForumManagementBlockedMembersVm(
          allBlockedMembers.OrderByDescending(p => p.BlockedOn).Take(10).ToList(), user);

      return forumManagementIndexVm;
    }

    private async Task<ForumManagementStatisticsVm> GetForumManagementStatisticsVm(List<Topic> lockedTopics,
      List<Thread> lockedThreads, List<Post> lockedPosts, List<Member> lockedMembers, ClaimsPrincipal user) {
      var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);

      return new ForumManagementStatisticsVm {
        BlockedMembersCount = lockedMembers.Count(),
        LockedTopicsCount = lockedTopics.Count(),
        LockedThreadsCount = lockedThreads.Count(),
        LockedPostsCount = lockedPosts.Count(),
        BlockedByUserMembersCount = lockedMembers.Count(m => m.BlockedBy == identityUser.Id),
        LockedByUserTopicsCount = lockedTopics.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserThreadsCount = lockedThreads.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserPostsCount = lockedPosts.Count(m => m.LockedBy == identityUser.Id)
      };
    }

    private async Task<List<ForumManagementLockedTopicVm>> GetForumManagementLockedTopicsVm(List<Topic> lockedTopics,
      ClaimsPrincipal user) {
      var topics = new List<ForumManagementLockedTopicVm>();

      foreach (var topic in lockedTopics) {
        var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);
        var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);

        topics.Add(new ForumManagementLockedTopicVm {
          TopicId = topic.Id,
          CreatedOn = topic.CreatedOn,
          LockedBy = lockedBy.UserName,
          LockedOn = topic.LockedOn,
          CreatedBy = createdBy.UserName,
          TopicText = topic.ContentText,
          IsAuthorizedForTopicEditLockAndDelete =
            await _authorizationService.IsAuthorizedForTopicEditLockAndDelete(topic, user)
        });
      }

      return topics;
    }

    private async Task<List<ForumManagementLockedThreadVm>> GetForumManagementLockedThreadsVm(
      List<Thread> lockedThreads,
      ClaimsPrincipal user) {
      var threads = new List<ForumManagementLockedThreadVm>();

      foreach (var thread in lockedThreads) {
        var lockedBy = await _userManager.FindByIdAsync(thread.LockedBy);
        var createdBy = await _userManager.FindByIdAsync(thread.CreatedBy);

        threads.Add(new ForumManagementLockedThreadVm {
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
        });
      }

      return threads;
    }

    private async Task<List<ForumManagementLockedPostVm>> GetForumManagementLockedPostsVm(List<Post> lockedPosts,
      ClaimsPrincipal user) {
      var posts = new List<ForumManagementLockedPostVm>();
      foreach (var post in lockedPosts) {
        var lockedBy = await _userManager.FindByIdAsync(post.LockedBy);
        var createdBy = await _userManager.FindByIdAsync(post.CreatedBy);

        posts.Add(new ForumManagementLockedPostVm {
          ThreadId = post.Thread,
          PostId = post.Id,
          CreatedOn = post.CreatedOn,
          LockedBy = lockedBy.UserName,
          LockedOn = post.LockedOn,
          CreatedBy = createdBy.UserName,
          PostText = post.ContentText,
          IsAuthorizedForPostEditAndDelete = await _authorizationService.IsAuthorizedForPostEditAndDelete(post, user),
          IsAuthorizedForPostLock = await _authorizationService.IsAuthorizedForPostLock(post, user)
        });
      }

      return posts;
    }

    private async Task<List<ForumManagementBlockedMemberVm>> GetForumManagementBlockedMembersVm(
      List<Member> blockedMembers,
      ClaimsPrincipal user) {
      var members = new List<ForumManagementBlockedMemberVm>();

      foreach (var member in blockedMembers) {
        var blockedBy = await _userManager.FindByIdAsync(member.BlockedBy);
        var identityUser = await _userManager.FindByIdAsync(member.Id);
        var roles = await _userManager.GetRolesAsync(identityUser);

        members.Add(new ForumManagementBlockedMemberVm {
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
        });
      }

      return members;
    }
  }
}