using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.TopicViewModels {
  public class TopicsIndexVm {
    public List<TopicsIndexTopicVm> Topics { get; set; }
    public List<TopicsIndexPostVm> LatestPosts { get; set; }
    public List<TopicsIndexThreadVm> LatestThreads { get; set; }

    public int TotalPosts { get; set; }
    public int TotalMembers { get; set; }
    public string NewestMember { get; set; }
  }

  public class TopicsIndexTopicVm {
    public int TopicId { get; set; }
    public string TopicText { get; set; }
    public string LockedBy { get; set; }
    public int ThreadCount { get; set; }
    public int PostCount { get; set; }
    public TopicsIndexThreadVm LatestActiveThread { get; set; }
  }

  public class TopicsIndexPostVm {
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime LatestCommentTime { get; set; }
    public string LatestCommenter { get; set; }
  }

  public class TopicsIndexThreadVm {
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
  }
}
