using System.Collections.Generic;
using Forum.Models.Pagination;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementBlockedMembersVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementBlockedMemberVm> BlockedMembers { get; set; }
    public Pager Pager { get; set; }
  }
}