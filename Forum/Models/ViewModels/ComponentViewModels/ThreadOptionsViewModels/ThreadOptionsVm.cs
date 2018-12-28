using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ComponentViewModels.ThreadOptionsViewModels {
  public class ThreadOptionsVm {
    public DateTime? LockedOn { get; set; }
    public int TopicId { get; set; }
    public int ThreadId { get; set; }
    public bool IsAuthorizedForThreadEdit { get; set; }
    public bool IsAuthorizedForThreadDelete { get; set; }
    public bool IsAuthorizedForThreadLock { get; set; }
  }
}
