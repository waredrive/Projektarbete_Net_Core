using System;

namespace Forum.Models.ViewModels.ForumManagementViewModels {
  public class ForumManagementBlockedMemberVm {
    public string Username { get; set; }
    public string MemberId { get; set; }
    public DateTime CreatedOn { get; set; }
    public string BlockedBy { get; set; }
    public DateTime? BlockedOn { get; set; }
    public DateTime? BlockEnd { get; set; }
    public string[] Roles { get; set; }
    public bool IsAuthorizedForProfileEdit { get; set; }
    public bool IsAuthorizedForProfileDelete { get; set; }
    public bool IsAuthorizedProfileBlock { get; set; }
    public bool IsAuthorizedProfileChangeRole { get; set; }
  }
}