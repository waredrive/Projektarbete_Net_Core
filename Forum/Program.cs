using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add deletion timeout for posts and threads.
    //TODO: Add custom messages to AccessDenied
    //TODO: Add returnurl to access denied and other relevant pages.
    //TODO: Add admin management page with list of blocked users/topics/threads/posts.
    //TODO: Add pagination.
    //TODO: Add id to posts so users can go to them from ex. latest posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Styling
    //TODO: Add seed of Default deleted user
    //TODO: Add SuperAdmin?
    //TODO: Add areas?
    //TODO: REFACTORING!!!

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}