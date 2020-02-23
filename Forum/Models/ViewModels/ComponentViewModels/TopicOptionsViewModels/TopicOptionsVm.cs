using System;

namespace Forum.Models.ViewModels.ComponentViewModels.TopicOptionsViewModels {
  public class TopicOptionsVm {
    public string OnRemoveReturnUrl { get; set; }
    public string ReturnUrl { get; set; }
    public DateTime? LockedOn { get; set; }
    public int TopicId { get; set; }
    public bool IsAuthorizedForTopicEditLockAndDelete { get; set; }
  }
}