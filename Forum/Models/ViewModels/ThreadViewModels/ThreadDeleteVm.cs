using System;

namespace Forum.Models.ViewModels.ThreadViewModels {
  public class ThreadDeleteVm {
    public int ThreadId { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int PostCount { get; set; }
    public string ThreadText { get; set; }
  }
}