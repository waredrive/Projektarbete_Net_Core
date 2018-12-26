using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.PostViewModels {
  public class PostsIndexVm {
    public string Thread { get; set; }
    public bool IsThreadLocked { get; set; }
    public List<PostsIndexPostVm> Posts { get; set; }
    public bool IsAuthorizedForPostCreate { get; set; }
  }

  public class PostsIndexPostVm {
    public int PostId { get; set; }
    public string PostText { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string LockedBy { get; set; }
    public string EditedBy { get; set; }
    public bool IsAuthorizedForPostEditAndDelete { get; set; }
    public bool IsAuthorizedForPostLock { get; set; }
  }
}
