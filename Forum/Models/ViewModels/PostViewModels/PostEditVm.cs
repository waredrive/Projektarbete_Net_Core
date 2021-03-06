﻿using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.PostViewModels {
  public class PostEditVm {
    [Required]
    public int PostId { get; set; }

    [Required]
    public int ThreadId { get; set; }

    [Required]
    [Display(Name = "Post Text")]
    public string PostText { get; set; }
  }
}