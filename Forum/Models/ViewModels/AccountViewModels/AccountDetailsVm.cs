using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountDetailsVm {
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
