using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Forum.Models.ViewModels.PostViewModels;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadCreateVm {
    [Required]
    public int TopicId { get; set; }
    [Required]
    [Display(Name = "Thread Text")]
    [StringLength(80, ErrorMessage = "The length of the Thread must be less than 80 characters.")]
    public string ThreadText { get; set; }

    public PostCreateVm Post { get; set; }
  }
}
