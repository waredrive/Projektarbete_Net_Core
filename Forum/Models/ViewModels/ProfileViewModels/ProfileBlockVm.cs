using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ProfileViewModels
{
    public class ProfileBlockVm
    {
      public string Username { get; set; }
    public string BlockedBy { get; set; }
      [DataType(DataType.Date)]
      [Display(Name = "Block until")]
      public DateTime? BlockedEnd { get; set; }
  }
}
