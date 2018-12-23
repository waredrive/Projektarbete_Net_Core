using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.PostViewModels
{
    public class PostLockVm
    {
      public int PostId { get; set; }
      public string CreatedBy { get; set; }
      public DateTime CreatedOn { get; set; }
      public string PostText { get; set; }
  }
}
