using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountEditVm {
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
