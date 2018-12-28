using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add custom messages to AccessDenied (Use TempData).
    //TODO: Create Partial views.
    //TODO: Refactor for less I/O to database - Use forumManagementService as example.
    //TODO: Show site navigation when in Topic/Thread - use bootstrap breadcrumbs.
    //TODO: Add Unblock/Block, Edit and Delete buttons when in thread/topic.
    //TODO: Add back buttons in threads and topics.
    //TODO: Prevent pages add to history, for omitting when back button is pressed.
    //TODO: Change Block to lock for consistency.
    //TODO: Add admin management page with list of blocked users/topics/threads/posts.
    //TODO: Add pagination.
    //TODO: Add id to posts and threads so users can go to them from ex. latest posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Styling
    //TODO: Add seed of Default deleted user if possible?
    //TODO: Add deletion timeout for posts and threads?
    //TODO: Add SuperAdmin?
    //TODO: Add areas?
    //TODO: REFACTORING!!!

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}