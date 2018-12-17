using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.TopicViewModels {
  public class TopicCreateVM {
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    [Required]
    [Display(Name="Topic Text")]
    public string CreatedText { get; set; }
  }
}
