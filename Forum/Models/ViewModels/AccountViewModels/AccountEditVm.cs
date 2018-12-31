using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountEditVm {
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; }

    [Required]
    [Display(Name = "Firstname")]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Lastname")]
    public string LastName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Birthdate")]
    public DateTime Birthdate { get; set; }
  }
}