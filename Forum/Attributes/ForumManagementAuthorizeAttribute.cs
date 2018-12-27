using Forum.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Forum.Attributes {
  public class ForumManagementAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
      var user = context.HttpContext.User;

      if (!user.Identity.IsAuthenticated) return;

      var authorizationService = context.HttpContext.RequestServices.GetService<AuthorizationService>();
      if (authorizationService.IsAuthorizedForForumManagement(user).Result)
        return;

      context.Result = new RedirectResult("Account/AccessDenied");
    }
  }
}