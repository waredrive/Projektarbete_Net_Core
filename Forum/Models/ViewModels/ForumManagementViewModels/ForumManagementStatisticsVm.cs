using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementStatisticsVm {
    public string UserName { get; set; }
    public int BlockedMembersCount { get; set; }
    public int LockedTopicsCount { get; set; }
    public int LockedThreadsCount { get; set; }
    public int LockedPostsCount { get; set; }
    public int BlockedByUserMembersCount { get; set; }
    public int LockedByUserTopicsCount { get; set; }
    public int LockedByUserThreadsCount { get; set; }
    public int LockedByUserPostsCount { get; set; }
  }
}
