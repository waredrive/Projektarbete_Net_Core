using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountAccessDeniedVm {
    private string _message;
    public string ReturnUrl { get; set; }
    public string Message {
      get => _message ?? "You do not have access to this resource.";
      set => _message = value;
    }
  }
}
