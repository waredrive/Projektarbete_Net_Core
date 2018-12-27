using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.ProfileViewModels {
  public class ProfileBlockVm {
    public string Username { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Block until")]
    public DateTime? BlockedEnd { get; set; }
  }
}