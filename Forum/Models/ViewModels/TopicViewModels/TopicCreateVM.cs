using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.TopicViewModels {
  public class TopicCreateVm {
    [Required]
    [Display(Name = "Topic Text")]
    [StringLength(50, ErrorMessage = "The length of the Topic must be less than 80 characters.")]
    public string TopicText { get; set; }
  }
}