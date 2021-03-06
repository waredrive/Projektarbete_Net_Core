﻿using System;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadUnlockVm {
    public int ThreadId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string LockedBy { get; set; }
    public DateTime LockedOn { get; set; }
    public int PostCount { get; set; }
    public string ThreadText { get; set; }
  }
}