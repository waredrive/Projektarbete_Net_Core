using System;

namespace Forum.Models.ViewModels.ComponentViewModels.ThreadOptionsViewModels {
  public class ThreadOptionsVm {
    public string OnRemoveReturnUrl { get; set; }
    public string ReturnUrl { get; set; }
    public DateTime? LockedOn { get; set; }
    public int TopicId { get; set; }
    public int ThreadId { get; set; }
    public bool IsAuthorizedForThreadEdit { get; set; }
    public bool IsAuthorizedForThreadDelete { get; set; }
    public bool IsAuthorizedForThreadLock { get; set; }
  }
}