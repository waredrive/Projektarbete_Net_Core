using System;
using System.Collections.Generic;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementIndexVm {
    public int BlockedMembersCount { get; set; }
    public int LockedTopicsCount { get; set; }
    public int LockedThreadsCount { get; set; }
    public int LockedPostsCount { get; set; }
    public int BlockedByUserMembersCount { get; set; }
    public int LockedByUserTopicsCount { get; set; }
    public int LockedByUserThreadsCount { get; set; }
    public int LockedByUserPostsCount { get; set; }
    public List<ForumManagementIndexBlockedUserVm> LatestBlockedUsers { get; set; }
    public List<ForumManagementIndexLockedTopicVm> LatestLockedTopics { get; set; }
    public List<ForumManagementIndexLockedThreadVm> LatestLockedThreads { get; set; }
    public List<ForumManagementIndexLockedPostVm> LatestLockedPosts { get; set; }
  }

  public class ForumManagementIndexBlockedUserVm {
    public string Username { get; set; }
    public string MemberId { get; set; }
    public DateTime CreatedOn { get; set; }
    public string BlockedBy { get; set; }
    public DateTime? BlockedOn { get; set; }
    public DateTime? BlockEnd { get; set; }
    public string[] Roles { get; set; }
    public bool IsAuthorizedForProfileEdit { get; set; }
    public bool IsAuthorizedForProfileDelete { get; set; }
    public bool IsAuthorizedProfileBlock { get; set; }
    public bool IsAuthorizedProfileChangeRole { get; set; }
  }

  public class ForumManagementIndexLockedTopicVm {
    public int TopicId { get; set; }
    public string TopicText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForTopicEditLockAndDelete { get; set; }
  }

  public class ForumManagementIndexLockedThreadVm {
    public int TopicId { get; set; }
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForThreadEdit { get; set; }
    public bool IsAuthorizedForThreadLock { get; set; }
    public bool IsAuthorizedForThreadDelete { get; set; }
  }

  public class ForumManagementIndexLockedPostVm {
    public int ThreadId { get; set; }
    public int PostId { get; set; }
    public string PostText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForPostEditAndDelete { get; set; }
    public bool IsAuthorizedForPostLock { get; set; }
  }
}