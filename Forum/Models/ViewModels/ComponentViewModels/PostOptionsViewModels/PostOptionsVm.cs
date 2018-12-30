using System;

namespace Forum.Models.ViewModels.ComponentViewModels.PostOptionsViewModels {
  public class PostOptionsVm {
    public string ReturnUrl { get; set; }
    public DateTime? LockedOn { get; set; }
    public int ThreadId { get; set; }
    public int PostId { get; set; }
    public bool IsAuthorizedForPostEditAndDelete { get; set; }
    public bool IsAuthorizedForPostLock { get; set; }
  }
}