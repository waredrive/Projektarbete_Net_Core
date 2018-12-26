using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Prevent going back to create pages when back button is clicked.
    //TODO: Add deletion timeout for posts and threads.
    //TODO: Validate if birthdate set wrongly
    //TODO: Revisit the can create methods and check if blocked admin can make changes (make it consistant).
    //TODO: Add returnurl to access denied and other relevant pages.
    //TODO: Add admin management page with list of blocked users/topics/threads/posts.
    //TODO: Add pagination.
    //TODO: Add id to posts so users can go to them from ex. latest posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Add SuperAdmin?
    //TODO: Add areas?

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}