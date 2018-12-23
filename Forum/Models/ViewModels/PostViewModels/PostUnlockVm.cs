using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.PostViewModels
{
    public class PostUnlockVm
    {
      public int PostId { get; set; }
      public string CreatedBy { get; set; }
      public DateTime CreatedOn { get; set; }
      public string LockedBy { get; set; }
      public DateTime LockedOn { get; set; }
    public string PostText { get; set; }
  }
}
