using System.Collections.Generic;
using Forum.Models.Pagination;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedTopicsVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementLockedTopicVm> LockedTopics { get; set; }
    public Pager Pager { get; set; }
  }
}