using System;

namespace Forum.Models.ViewModels.PostViewModels {
  public class PostDeleteVm {
    public int PostId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string PostText { get; set; }
  }
}