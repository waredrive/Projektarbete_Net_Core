﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Forum.Models.ViewModels.AccountViewModels {
  public class AccountRegisterVm {
    [Required]
    [EmailAddress]
    [Display(Name = "E-mail")]
    public string Email { get; set; }

    [Required]
    [StringLength(18)]
    [Display(Name = "Username")]
    public string UserName { get; set; }

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

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
      MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
  }
}