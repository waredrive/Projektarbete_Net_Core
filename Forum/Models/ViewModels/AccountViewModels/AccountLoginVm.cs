using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountLoginVm {
    [Required]
    [Display(Name = "Username")]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }
}