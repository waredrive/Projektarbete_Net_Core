using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add topic options VM to every topic VM for checking is methods directly in ef linq query.
    //TODO: Save profileimage directly to vm for less sql I/O.
    //TODO: Change deleted members view of posts and roles in posts.
    //TODO: Replace magic string in returnToAction with something better.
    //TODO: Add not found to thread and posts to check if topic and thread exist. Use modal to display fail.
    //TODO: Check how to solve without access denied thread sleep.
    //TODO: Don't show deleted users posts and role in profile.
    //TODO: Return values from services to act upon in controllers (i.e. if an entity is not added, display modal failed), can use savechangesasync for this.
    //TODO: https://www.smarterasp.net
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