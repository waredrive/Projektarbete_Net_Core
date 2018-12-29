﻿using System;
using System.Collections.Generic;
using Forum.Models.ViewModels.SharedViewModels.PaginationViewModels;

namespace Forum.Models.ViewModels.PostViewModels {
  public class PostsIndexVm {
    public int TopicId { get; set; }
    public string TopicText { get; set; }
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public bool IsThreadLocked { get; set; }
    public List<PostsIndexPostVm> Posts { get; set; }
    public bool IsAuthorizedForPostCreate { get; set; }
    public PaginationVm Pagination { get; set; }
  }

  public class PostsIndexPostVm {
    public int PostId { get; set; }
    public string PostText { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public string EditedBy { get; set; }
    public DateTime? EditedOn { get; set; }
    public string[] CreatorsRoles { get; set; }
    public int CreatorsTotalposts { get; set; }
    public string CreatorsProfileImage { get; set; }
  }
}