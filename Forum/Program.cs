using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add custom messages to AccessDenied (Use TempData).
    //TODO: Make statistics in forumManager Sticky
    //TODO: Create Partial views.
    //TODO: Prevent pages add to history, for omitting when back button is pressed. Only "nested" pages - Profile and Account.
    //TODO: Add pagination.
    //TODO: Add id to posts and threads so users can go to them from ex. latest posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Styling
    //TODO: Add seed of Default deleted user if possible?
    //TODO: Add deletion timeout for posts and threads?
    //TODO: Add SuperAdmin?
    //TODO: Add areas?
    //TODO: REFACTORING!!! - Check if all async methods are ending with Async

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}