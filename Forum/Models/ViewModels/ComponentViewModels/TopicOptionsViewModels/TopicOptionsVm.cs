using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.ComponentViewModels.TopicOptionsViewModels
{
    public class TopicOptionsVm
    {
      public DateTime? LockedOn { get; set; }
      public int TopicId { get; set; }
      public bool IsAuthorizedForTopicEditLockAndDelete { get; set; }
  }
}
