using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ComponentViewModels.NavbarViewModels {
  public class NavbarVm {
    public string ProfileImage { get; set; }
    public bool IsAuthorizedForForumManagement { get; set; }
  }
}
