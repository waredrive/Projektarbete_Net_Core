using System;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementLockedPostVm {
    public int ThreadId { get; set; }
    public int PostId { get; set; }
    public string PostText { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public string CreatorProfileImage { get; set; }
    public string LockerProfileImage { get; set; }
    public string LockedBy { get; set; }
    public DateTime? LockedOn { get; set; }
    public bool IsAuthorizedForPostEditAndDelete { get; set; }
    public bool IsAuthorizedForPostLock { get; set; }
  }
}