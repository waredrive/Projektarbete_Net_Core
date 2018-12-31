using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add Typeahead with ajax call for searching.
    //TODO: see if not change how images are passed to client.
    //TODO: Don't show headlines in forum management if tables are empty.
    //TODO: Add confirmations when adding, deleting, editing, etc. Maybe modal?
    //TODO: Add seed of Default deleted user if possible?
    //TODO: Add partial views.
    //TODO: Add timeout on edit and delete posts, threads.
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