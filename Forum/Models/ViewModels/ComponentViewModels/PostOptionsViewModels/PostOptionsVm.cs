using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ComponentViewModels.PostOptionsViewModels {
  public class PostOptionsVm {
    public DateTime? LockedOn { get; set; }
    public int PostId { get; set; }
    public bool IsAuthorizedForPostEditAndDelete { get; set; }
    public bool IsAuthorizedForPostLock { get; set; }
  }
}
