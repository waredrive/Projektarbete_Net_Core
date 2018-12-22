﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.ViewModels.PostViewModel;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadEditVm {
    [Required]
    public int ThreadId { get; set; }
    [Required]
    public int TopicId { get; set; }
    [Required]
    [Display(Name = "Thread Text")]
    [StringLength(80, ErrorMessage = "The length of the Thread must be less than 80 characters.")]
    public string ThreadText { get; set; }
  }
}
