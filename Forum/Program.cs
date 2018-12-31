using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add modal also to not found and maybe add a method for creating tempdata easier.
    //TODO: Still problems with back button if form wrong more than two times. maybe on back, go to details and pass the returnUrl...
    //TODO: Do not update if sending no new data. All edits/updates.
    //TODO: Add seed of Default deleted user if possible?
    //TODO: Add SuperAdmin? - if so Admin cannot add SuperAdmin role and manipulate SuperAdmin.
    //TODO: Add areas?
    //TODO: A member can include idnavigation! Change all redundant searches for identityuser.
    //TODO: REFACTORING!!! - Check if all async methods are ending with Async

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}