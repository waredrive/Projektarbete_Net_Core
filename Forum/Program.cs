using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Go to specific Deleted page when clicking on deleted username.
    //TODO: A member can include idnavigation! Change all redundant searches for identityuser.
    //TODO: Create Partial views.
    //TODO: Add confirmations when adding, deleting, editing, etc. Maybe modal?
    //TODO: Add custom messages to AccessDenied (Use TempData).
    //TODO: There are som redundant ReturnUrl in code. Remove or use!
    //TODO: Prevent pages add to history, for omitting when back button is pressed. Only "nested" pages - Profile and Account.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Add seed of Default deleted user if possible?
    //TODO: Add deletion timeout for posts and threads?
    //TODO: Add SuperAdmin? - if so Admin cannot add SuperAdmin role and manipulate SuperAdmin.
    //TODO: Add areas?
    //TODO: REFACTORING!!! - Check if all async methods are ending with Async

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}