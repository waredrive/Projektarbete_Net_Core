using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadCreateVm {
    [Required]
    public int TopicId { get; set; }
    [Required]
    [Display(Name = "Thread Text")]
    public string ThreadText { get; set; }
  }
}
