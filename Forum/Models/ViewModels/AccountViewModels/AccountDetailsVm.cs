using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountDetailsVm {
    public string UserName { get; set; }

    [Display(Name = "E-mail")]
    public string Email { get; set; }

    [Display(Name = "Firstname")]
    public string FirstName { get; set; }

    [Display(Name = "Lastname")]
    public string LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTime Birthdate { get; set; }

    public bool IsAuthorizedForAccountEdit { get; set; }
  }
}