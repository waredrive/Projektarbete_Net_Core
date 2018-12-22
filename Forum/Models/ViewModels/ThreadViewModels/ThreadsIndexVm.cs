using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadsIndexVm {
    public string Topic { get; set; }
    public bool IsTopicLocked { get; set; }
    public List<ThreadsIndexThreadVm> Threads { get; set; }
  }

  public class ThreadsIndexThreadVm { 
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string LockedBy { get; set; }
    public string RemovedBy { get; set; }
    public string EditedBy { get; set; }
    public int PostCount { get; set; }
    public bool IsUserAuthorized { get; set; }
  }
}
