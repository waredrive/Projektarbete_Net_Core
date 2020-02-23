using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.Pagination;
using Forum.Models.ViewModels.ForumManagementViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ForumManagementService {
    private readonly AuthorizationService _authorizationService;
    private readonly ForumDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public ForumManagementService(
      UserManager<IdentityUser> userManager, ForumDbContext db, AuthorizationService authorizationService) {
      _userManager = userManager;
      _db = db;
      _authorizationService = authorizationService;
    }

    public async Task<ForumManagementIndexVm> GetForumManagementIndexVmAsync(ClaimsPrincipal user) {
      var allBlockedMembers = await _db.Member.Where(m => m.BlockedBy != null).ToListAsync();
      var allLockedTopics = await _db.Topic.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedThreads = await _db.Thread.Where(m => m.LockedBy != null).ToListAsync();
      var allLockedPosts = await _db.Post.Include(p => p.ThreadNavigation).Where(m => m.LockedBy != null).ToListAsync();

      var forumManagementIndexVm = new ForumManagementIndexVm {
        LatestBlockedMembers = new List<ForumManagementBlockedMemberVm>(),
        LatestLockedPosts = new List<ForumManagementLockedPostVm>(),
        LatestLockedThreads = new List<ForumManagementLockedThreadVm>(),
        LatestLockedTopics = new List<ForumManagementLockedTopicVm>(),
        Statistics = await GetForumManagementStatisticsVmAsync(user.Identity.Name)
      };

      forumManagementIndexVm.LatestLockedTopics =
        await GetForumManagementLockedTopicVmsAsync(
          allLockedTopics.OrderByDescending(t => t.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestLockedThreads =
        await GetForumManagementLockedThreadVmsAsync(
          allLockedThreads.OrderByDescending(t => t.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestLockedPosts =
        await GetForumManagementLockedPostVmsAsync(allLockedPosts.OrderByDescending(p => p.LockedOn).Take(10).ToList(),
          user);

      forumManagementIndexVm.LatestBlockedMembers =
        await GetForumManagementBlockedMemberVmsAsync(
          allBlockedMembers.OrderByDescending(p => p.BlockedOn).Take(10).ToList(), user);

      return forumManagementIndexVm;
    }

    private async Task<ForumManagementStatisticsVm> GetForumManagementStatisticsVmAsync(string username) {
      var identityUser = await _userManager.FindByNameAsync(username);

      return new ForumManagementStatisticsVm {
        UserName = identityUser.UserName,
        BlockedMembersCount = _db.Member.Count(m => m.BlockedBy != null),
        LockedTopicsCount = _db.Topic.Count(m => m.LockedBy != null),
        LockedThreadsCount = _db.Thread.Count(m => m.LockedBy != null),
        LockedPostsCount = _db.Post.Count(m => m.LockedBy != null),
        BlockedByUserMembersCount = _db.Member.Count(m => m.BlockedBy == identityUser.Id),
        LockedByUserTopicsCount = _db.Topic.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserThreadsCount = _db.Thread.Count(m => m.LockedBy == identityUser.Id),
        LockedByUserPostsCount = _db.Post.Count(m => m.LockedBy == identityUser.Id)
      };
    }

    private async Task<List<ForumManagementLockedTopicVm>> GetForumManagementLockedTopicVmsAsync(
      List<Topic> lockedTopics,
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
            await _authorizationService.IsAuthorizedForTopicEditLockAndDeleteAsync(topic, user)
        });
      }

      return topics;
    }

    private async Task<List<ForumManagementLockedThreadVm>> GetForumManagementLockedThreadVmsAsync(
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
            await _authorizationService.IsAuthorizedForThreadEditAsync(thread, user),
          IsAuthorizedForThreadLock = await _authorizationService.IsAuthorizedForThreadLockAsync(thread, user),
          IsAuthorizedForThreadDelete = await _authorizationService.IsAuthorizedForThreadDeleteAsync(thread, user)
        });
      }

      return threads;
    }

    private async Task<List<ForumManagementLockedPostVm>> GetForumManagementLockedPostVmsAsync(List<Post> lockedPosts,
      ClaimsPrincipal user) {
      var posts = new List<ForumManagementLockedPostVm>();
      foreach (var post in lockedPosts) {
        var lockedBy = await _userManager.FindByIdAsync(post.LockedBy);
        var createdBy = await _userManager.FindByIdAsync(post.CreatedBy);

        posts.Add(new ForumManagementLockedPostVm {
          TopicId = post.ThreadNavigation.Topic,
          ThreadId = post.Thread,
          PostId = post.Id,
          CreatedOn = post.CreatedOn,
          LockedBy = lockedBy.UserName,
          LockedOn = post.LockedOn,
          CreatedBy = createdBy.UserName,
          ThreadText = post.ThreadNavigation.ContentText,
          IsAuthorizedForPostEditAndDelete =
            await _authorizationService.IsAuthorizedForPostEditAndDeleteAsync(post, user),
          IsAuthorizedForPostLock = await _authorizationService.IsAuthorizedForPostLockAsync(post, user)
        });
      }

      return posts;
    }

    private async Task<List<ForumManagementBlockedMemberVm>> GetForumManagementBlockedMemberVmsAsync(
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
            await _authorizationService.IsAuthorizedForAccountAndProfileEditAsync(identityUser.UserName, user),
          IsAuthorizedForProfileDelete =
            await _authorizationService.IsAuthorizedForProfileDeleteAsync(identityUser.UserName, user),
          IsAuthorizedProfileBlock =
            await _authorizationService.IsAuthorizedProfileBlockAsync(identityUser.UserName, user),
          IsAuthorizedProfileChangeRole =
            await _authorizationService.IsAuthorizedProfileChangeRoleAsync(identityUser.UserName, user)
        });
      }

      return members;
    }

    public async Task<ForumManagementLockedTopicsVm> GetForumManagementLockedTopicsVmAsync(ClaimsPrincipal user,
      int currentPage, string username = null, int pageSize = 20) {
      List<Topic> lockedTopics;
      IdentityUser identityUser = null;

      if (username != null) {
        identityUser = await _userManager.FindByNameAsync(username);
        lockedTopics = await _db.Topic.Where(t => t.LockedBy == identityUser.Id).OrderBy(t => t.LockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      } else {
        lockedTopics = await _db.Topic.Where(t => t.LockedBy != null).OrderBy(t => t.LockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      }

      return new ForumManagementLockedTopicsVm {
        Pager = await GetPaginationVmForLockedTopics(currentPage, pageSize),
        UserName = identityUser?.UserName,
        Statistics = await GetForumManagementStatisticsVmAsync(identityUser?.UserName ?? user.Identity.Name),
        LockedTopics = await GetForumManagementLockedTopicVmsAsync(lockedTopics, user)
      };
    }

    private async Task<Pager> GetPaginationVmForLockedTopics(int currentPage, int pageSize) {
      var totalItems = await _db.Topic.Where(t => t.LockedBy != null).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }

    public async Task<ForumManagementLockedThreadsVm> GetForumManagementLockedThreadsVmAsync(ClaimsPrincipal user,
      int currentPage, string username = null, int pageSize = 20) {
      List<Thread> lockedThreads;
      IdentityUser identityUser = null;

      if (username != null) {
        identityUser = await _userManager.FindByNameAsync(username);
        lockedThreads = await _db.Thread.Where(t => t.LockedBy == identityUser.Id).OrderBy(t => t.LockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      } else {
        lockedThreads = await _db.Thread.Where(t => t.LockedBy != null).OrderBy(t => t.LockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      }

      return new ForumManagementLockedThreadsVm {
        Pager = await GetPaginationVmForLockedThreads(currentPage, pageSize),
        UserName = identityUser?.UserName,
        Statistics = await GetForumManagementStatisticsVmAsync(identityUser?.UserName ?? user.Identity.Name),
        LockedThreads = await GetForumManagementLockedThreadVmsAsync(lockedThreads, user)
      };
    }

    private async Task<Pager> GetPaginationVmForLockedThreads(int currentPage, int pageSize) {
      var totalItems = await _db.Thread.Where(t => t.LockedBy != null).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }

    public async Task<ForumManagementLockedPostsVm> GetForumManagementLockedPostsVmAsync(ClaimsPrincipal user,
      int currentPage, string username = null, int pageSize = 20) {
      List<Post> lockedPosts;
      IdentityUser identityUser = null;

      if (username != null) {
        identityUser = await _userManager.FindByNameAsync(username);
        lockedPosts = await _db.Post.Include(p => p.ThreadNavigation).Where(p => p.LockedBy == identityUser.Id)
          .OrderBy(p => p.LockedOn).Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      } else {
        lockedPosts = await _db.Post.Include(p => p.ThreadNavigation).Where(p => p.LockedBy != null)
          .OrderBy(p => p.LockedOn).Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      }

      return new ForumManagementLockedPostsVm {
        Pager = await GetPaginationVmForLockedPosts(currentPage, pageSize),
        UserName = identityUser?.UserName,
        Statistics = await GetForumManagementStatisticsVmAsync(identityUser?.UserName ?? user.Identity.Name),
        LockedPosts = await GetForumManagementLockedPostVmsAsync(lockedPosts, user)
      };
    }

    private async Task<Pager> GetPaginationVmForLockedPosts(int currentPage, int pageSize) {
      var totalItems = await _db.Post.Where(p => p.LockedBy != null).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }

    public async Task<ForumManagementBlockedMembersVm> GetForumManagementBlockedMembersVmAsync(ClaimsPrincipal user,
      int currentPage, string username = null, int pageSize = 20) {
      List<Member> blockedMembers;
      IdentityUser identityUser = null;

      if (username != null) {
        identityUser = await _userManager.FindByNameAsync(username);
        blockedMembers = await _db.Member.Where(m => m.BlockedBy == identityUser.Id).OrderBy(m => m.BlockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      } else {
        blockedMembers = await _db.Member.Where(m => m.BlockedBy != null).OrderBy(m => m.BlockedOn)
          .Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
      }

      return new ForumManagementBlockedMembersVm {
        Pager = await GetPaginationVmForBlockedMembers(currentPage, pageSize),
        UserName = identityUser?.UserName,
        Statistics = await GetForumManagementStatisticsVmAsync(identityUser?.UserName ?? user.Identity.Name),
        BlockedMembers = await GetForumManagementBlockedMemberVmsAsync(blockedMembers, user)
      };
    }

    private async Task<Pager> GetPaginationVmForBlockedMembers(int currentPage, int pageSize) {
      var totalItems = await _db.Member.Where(m => m.BlockedBy != null).CountAsync();
      return new Pager(totalItems, currentPage, pageSize);
    }
  }
}