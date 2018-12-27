using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Forum.Controllers;
using Forum.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Forum.Attributes {
  public class ForumManagementAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
      var user = context.HttpContext.User;

      if (!user.Identity.IsAuthenticated) {
        return;
      }

      var authorizationService = context.HttpContext.RequestServices.GetService<AuthorizationService>();
      if (authorizationService.IsAuthorizedForForumManagement(user).Result)
        return;

      context.Result =  new RedirectResult("Account/AccessDenied");
      return;
    }
  }
}
