using System.Collections.Generic;
using Forum.Models.Pagination;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedThreadsVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementLockedThreadVm> LockedThreads { get; set; }
    public Pager Pager { get; set; }
  }
}