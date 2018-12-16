using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.MVC.Models.TopicViewModels {
  public class TopicIndexVM {
    public List<TopicIndexTopicVM> Topics { get; set; }
    public List<TopicIndexPostVM> LatestPosts { get; set; }
    public List<TopicIndexThreadVM> LatestThreads { get; set; }

    public int TotalPosts { get; set; }
    public int TotalMembers { get; set; }
    public string NewestMember { get; set; }
  }

  public class TopicIndexTopicVM {
    public string TopicText { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public string RemovedBy { get; set; }
    public string EditedBy { get; set; }
    public int ThreadCount { get; set; }
    public int PostCount { get; set; }
    public TopicIndexThreadVM LatestActiveThread { get; set; }
  }

  public class TopicIndexPostVM {
    public string TopicText { get; set; }
    public DateTime LatestCommentTime { get; set; }
    public string LatestCommenter { get; set; }
  }

  public class TopicIndexThreadVM {
    public string ThreadText { get; set; }
    public DateTime CreatedTime { get; set; }
    public string CreatedBy { get; set; }
  }
}
