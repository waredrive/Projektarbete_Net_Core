using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ComponentViewModels.MemberOptionsViewModels
{
    public class MemberOptionsVm
    {
      public string ReturnUrl { get; set; }
      public string Username { get; set; }
      public DateTime? BlockedOn { get; set; }
      public bool IsUserOwner { get; set; }
      public bool IsAuthorizedForProfileEdit { get; set; }
      public bool IsAuthorizedForProfileDelete { get; set; }
      public bool IsAuthorizedProfileBlock { get; set; }
      public bool IsAuthorizedProfileChangeRole { get; set; }
  }
}
