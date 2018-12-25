using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forum.Models.ViewModels.ProfileViewModels
{
    public class ProfileRoleEditVm
    {
      public string Username { get; set; }
      [Display(Name = "Change Role")]
      public string Role { get; set; }
      public SelectListItem[] Roles { get; set; }
  }
}
