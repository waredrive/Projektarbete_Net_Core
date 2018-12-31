using System;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedTopicVm {
    public int TopicId { get; set; }
    public string TopicText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForTopicEditLockAndDelete { get; set; }
  }
}