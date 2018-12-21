using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadCreateVm {
    [Required]
    [Display(Name = "Thread Text")]
    public string ThreadText { get; set; }
  }
}
