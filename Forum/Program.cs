using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Remove block on signup if blockend (check if works).
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