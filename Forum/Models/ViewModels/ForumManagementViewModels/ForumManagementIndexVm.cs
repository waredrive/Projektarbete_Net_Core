using System.Collections.Generic;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementIndexVm {
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementBlockedMemberVm> LatestBlockedMembers { get; set; }
    public List<ForumManagementLockedTopicVm> LatestLockedTopics { get; set; }
    public List<ForumManagementLockedThreadVm> LatestLockedThreads { get; set; }
    public List<ForumManagementLockedPostVm> LatestLockedPosts { get; set; }
  }
}