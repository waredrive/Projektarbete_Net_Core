using Microsoft.AspNetCore.Authorization;

namespace Forum.Attributes {
  public class AuthorizeRolesAttribute : AuthorizeAttribute {
    public AuthorizeRolesAttribute(params string[] roles) {
      Roles = string.Join(",", roles);
    }
  }
}