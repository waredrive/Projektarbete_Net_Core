using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.MVC.Models.ViewModels {
  public class ThreadIndexVM {
    public List<ThreadIndexItemVM> Threads { get; set; }
  }

  public class ThreadIndexItemVM { 
    public string ThreadText { get; set; }
    public string CreatedBy { get; set; }
    public string LockedBy { get; set; }
    public string RemovedBy { get; set; }
    public string EditedBy { get; set; }
    public int PostCount { get; set; }
  }
}
