using System;
using System.Collections.Generic;

namespace Forum.Models.ViewModels.TopicViewModels {
  public class TopicsIndexVm {
    public List<TopicsIndexTopicVm> Topics { get; set; }
    public List<TopicsIndexPostVm> LatestPosts { get; set; }
    public List<TopicsIndexLatestThreadVm> LatestThreads { get; set; }
    public bool IsAuthorizedForTopicCreate { get; set; }
  }

  public class TopicsIndexTopicVm {
    public int TopicId { get; set; }
    public string TopicText { get; set; }
    public string LockedBy { get; set; }
    public int ThreadCount { get; set; }
    public int PostCount { get; set; }
    public TopicsIndexThreadVm LatestThreadPostedTo { get; set; }
  }

  public class TopicsIndexPostVm {
    public int ThreadId { get; set; }
    public int PostId { get; set; }
    public string ThreadText { get; set; }
    public DateTime LatestCommentTime { get; set; }
    public string LatestCommenter { get; set; }
  }

  public class TopicsIndexThreadVm {
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime? LatestCommentTime { get; set; }
    public string LatestCommenter { get; set; }
  }

  public class TopicsIndexLatestThreadVm {
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string CreatedBy { get; set; }
  }
}