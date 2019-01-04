using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: When user deletes itself, the tempdata model message is still persistant when navigating to "/".
    //TODO: Write a method that sets ViewBag.ReturnUrl.
    //TODO: Add not found to thread and posts to check if topic and thread exist. Use modal to display fail.
    //TODO: Return values from services to act upon in controllers (i.e. if an entity is not added, display modal failed), can use savechangesasync for this. Include authorization in those values.
    //TODO: Do not update if sending no new data. All edits/updates. Use own validationattribute and an "old topic/ old thread/ old posts" properties.
    //TODO: A member can include idnavigation! Change all redundant searches for identityuser.
    //TODO: REFACTORING!!! - Check if all async methods are ending with Async

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}