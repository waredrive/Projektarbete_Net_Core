using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Attributes;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ForumManagementViewModels;
using Forum.Models.ViewModels.TopicViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services
{
    public class ForumManagementService
    {
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
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        var users = await _userManager.GetUsersInRoleAsync(Roles.User);
        var moderators = await _userManager.GetUsersInRoleAsync(Roles.Moderator);


      var forumManagementIndexVm = new ForumManagementIndexVm {
          LatestBlockedUsers = new List<ForumManagementIndexBlockedUserVm>(),
          LatestLockedPosts = new List<ForumManagementIndexLockedPostVm>(),
          LatestLockedThreads = new List<ForumManagementIndexLockedThreadVm>(),
          LatestLockedTopics = new List<ForumManagementIndexLockedTopicVm>(),
          UserCount= users.Count,
          AdminCount = admins.Count,
        ModeratorCount = moderators.Count,
        BlockedMembersCount = _db.Member.Count(m => m.BlockedBy != null),
        LockedTopicsCount = _db.Topic.Count(m => m.LockedBy != null ),
        LockedThreadsCount = _db.Thread.Count(m => m.LockedBy != null ),
        LockedPostsCount = _db.Post.Count(m => m.LockedBy != null )
        };


        var lockedTopics = _db.Topic.Where(t => t.LockedBy != null).OrderByDescending(t => t.LockedOn).Take(10);

        foreach (var lockedTopic in lockedTopics) {
          forumManagementIndexVm.LatestLockedTopics.Add(await GetForumManagementIndexLockedTopicVm(lockedTopic, user));
        }

        var lockedThreads = _db.Thread.Where(t => t.LockedBy != null).OrderByDescending(t => t.LockedOn).Take(10);

        foreach (var lockedThread in lockedThreads) {
          forumManagementIndexVm.LatestLockedThreads.Add(await GetForumManagementIndexLockedThreadVm(lockedThread, user));
        }

        var lockedPosts = _db.Post.Where(p => p.LockedBy != null).OrderByDescending(p => p.LockedOn).Take(10);

        foreach (var lockedPost in lockedPosts) {
          forumManagementIndexVm.LatestLockedPosts.Add(await GetForumManagementIndexLockedPostVm(lockedPost, user));
        }

        var blockedUsers = _db.Member.Where(p => p.BlockedBy != null).OrderByDescending(p => p.BlockedOn).Take(10);

        foreach (var blockedUser in blockedUsers) {
          forumManagementIndexVm.LatestBlockedUsers.Add(await GetForumManagementIndexBlockedUserVm(blockedUser, user));
        }

        return forumManagementIndexVm;
      }

    private async Task<ForumManagementIndexLockedTopicVm> GetForumManagementIndexLockedTopicVm(Topic topic,
        ClaimsPrincipal user) {
        var lockedBy = await _userManager.FindByIdAsync(topic.LockedBy);
        var createdBy = await _userManager.FindByIdAsync(topic.CreatedBy);

        return new ForumManagementIndexLockedTopicVm() {
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

        return new ForumManagementIndexLockedThreadVm() {
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

        return new ForumManagementIndexLockedPostVm() {
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

      return new ForumManagementIndexBlockedUserVm() {
          MemberId = member.Id,
          CreatedOn = member.CreatedOn,
          BlockedBy = blockedBy.UserName,
          BlockedOn = member.BlockedOn,
          BlockEnd = member.BlockedEnd,
          Username = identityUser.UserName,
          Roles = roles.ToArray(),
          IsAuthorizedForProfileEdit = await _authorizationService.IsAuthorizedForAccountAndProfileEdit(identityUser.UserName, user),
          IsAuthorizedForProfileDelete = await _authorizationService.IsAuthorizedForProfileDelete(identityUser.UserName, user),
          IsAuthorizedProfileBlock = await _authorizationService.IsAuthorizedProfileBlock(identityUser.UserName, user),
          IsAuthorizedProfileChangeRole = await _authorizationService.IsAuthorizedProfileChangeRole(identityUser.UserName, user)
        };
      }


  }
}
