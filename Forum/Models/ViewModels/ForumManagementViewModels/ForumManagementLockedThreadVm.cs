using System;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedThreadVm {
    public int TopicId { get; set; }
    public int ThreadId { get; set; }
    public string ThreadText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForThreadEdit { get; set; }
    public bool IsAuthorizedForThreadLock { get; set; }
    public bool IsAuthorizedForThreadDelete { get; set; }
  }
}