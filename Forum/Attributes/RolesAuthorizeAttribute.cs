using Microsoft.AspNetCore.Authorization;

namespace Forum.Attributes {
  public class RolesAuthorizeAttribute : AuthorizeAttribute {
    public RolesAuthorizeAttribute(params string[] roles) {
      Roles = string.Join(",", roles);
    }
  }
}