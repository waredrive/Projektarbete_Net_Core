using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Policy;
using Microsoft.CodeAnalysis.CSharp;

namespace Forum.Models.Identity {
  public static class DeletedMember {

    public static string GetPassword() {
      return $"!A{Guid.NewGuid().ToString()}B!";
    }
    public static string GetUsername(int deletedUsersCount) {
      return $"{UsernamePrefix}-{++deletedUsersCount}";
    }
    public const string UsernamePrefix = "DELETED";
    public const string Email = "DELTED@DELETED.COM";
    public const string FirstName = "DELETED";
    public const string LastName = "DELETED";
    public static readonly DateTime BirthDate = new DateTime(1, 1, 1, 1, 1, 1);
  }
}