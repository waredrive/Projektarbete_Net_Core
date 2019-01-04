using Microsoft.AspNetCore.Mvc;

namespace Forum.Extensions {
  public static class ControllerBaseExtension {
    public static RedirectToActionResult RedirectToControllerAction<T>(this ControllerBase c, string actionName) where T : ControllerBase {
      return c.RedirectToAction(actionName, typeof(T).Name.Replace("Controller", string.Empty));
    }
  }
}
