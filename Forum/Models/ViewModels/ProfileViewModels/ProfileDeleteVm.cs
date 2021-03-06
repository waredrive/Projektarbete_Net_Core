﻿using System;

namespace Forum.Models.ViewModels.ProfileViewModels {
  public class ProfileDeleteVm {
    public string Username { get; set; }
    public string[] Roles { get; set; }
    public string ProfileImage { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? BlockedOn { get; set; }
    public string BlockedBy { get; set; }
    public DateTime? BlockedEnd { get; set; }
    public int TotalThreads { get; set; }
    public int TotalPosts { get; set; }
  }
}