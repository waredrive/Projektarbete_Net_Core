using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Forum.Models.ViewModels.ProfileViewModels {
  public class ProfileEditVm {
    public string Username { get; set; }

    [Required]
    [StringLength(18)]
    [Display(Name = "Username")]
    public string NewUsername { get; set; }

    [Display(Name = "Profile Image")]
    public IFormFile ProfileImage { get; set; }
  }
}