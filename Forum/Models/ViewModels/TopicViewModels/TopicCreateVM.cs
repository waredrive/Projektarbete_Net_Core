﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.TopicViewModels {
  public class TopicCreateVm {
    [Required]
    [Display(Name="Topic Text")]
    public string TopicText { get; set; }
  }
}
