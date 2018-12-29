using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.ViewModels.SharedViewModels.PaginationViewModels;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedTopicsVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementLockedTopicVm> LockedTopics { get; set; }
    public PaginationVm Pagination { get; set; }
  }
}
