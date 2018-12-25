using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models.ViewModels.ProfileViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Forum.Models.ViewModels.ComponentViewModels.AdminProfileEditViewModels
{
    public class AdminProfileEditVm
    {
      public ProfileRoleEditVm ProfileRoleEditVm { get; set; }
      public ProfileBlockVm ProfileBlockVm { get; set; }
  }
}
