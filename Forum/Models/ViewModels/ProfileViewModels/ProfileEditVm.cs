﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forum.Models.ViewModels.ProfileViewModels {
  public class ProfileEditVm {
    public string OldUsername { get; set; }
    [Required]
    [Display(Name = "Username")]
    public string NewUsername { get; set; }
    [Display(Name = "Profile Image")]
    public IFormFile ProfileImage { get; set; }
    public string Role { get; set; }
    public SelectListItem[] Roles { get; set; }
    public bool userIsOwner { get; set; }
  }
}
