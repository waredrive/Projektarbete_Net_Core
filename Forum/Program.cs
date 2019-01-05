using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //BUG: Login does send returnUrl to the action users tries to access. Problem is that the back button in that action uses the same returnurl so the back button redirect to itself (the same action user is on). Big problem when going to a user => loging in => removing the user (creates infinite loop of redirections). Must pass on new ReturnUrl from login to the new action.
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