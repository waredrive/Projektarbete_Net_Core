﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementBlockedMembersVm {
    public string UserName { get; set; }
    public ForumManagementStatisticsVm Statistics { get; set; }
    public List<ForumManagementBlockedMemberVm> BlockedMembers { get; set; }
    public Pagination.Pager Pager { get; set; }
  }
}
