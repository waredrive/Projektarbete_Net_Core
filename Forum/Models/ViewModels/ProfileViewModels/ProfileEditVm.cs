using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forum.Models.ViewModels.ProfileViewModels
{
    public class ProfileEditVm
    {
      [Required]
      [Display(Name = "Username")]
      public string Username { get; set; }
      public string ProfileImage { get; set; }
      public string Role { get; set; }
      public SelectListItem[] Roles { get; set; }
  }
}
