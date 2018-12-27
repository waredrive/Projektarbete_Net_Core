using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.ProfileViewModels {
  public class ProfileUnblockVm {
    public string Username { get; set; }
    public string BlockedBy { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Block until")]
    public DateTime? BlockedEnd { get; set; }

    public DateTime? BlockedOn { get; set; }
  }
}