using System.Collections.Generic;
using Forum.Models.Pagination;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedPostsVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementLockedPostVm> LockedPosts { get; set; }
    public Pager Pager { get; set; }
  }
}