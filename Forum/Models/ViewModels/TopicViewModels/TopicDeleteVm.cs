using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.TopicViewModels
{
    public class TopicDeleteVm
    {
      public int TopicId { get; set; }
      public string CreatedBy { get; set; }
      public DateTime CreatedOn { get; set; }
      public int ThreadCount { get; set; }
      public int PostCount { get; set; }
      public string TopicText { get; set; }
  }
}
