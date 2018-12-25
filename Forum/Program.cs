using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Prevent admins from editing other users Profiles. Admins should though be able to block, delete and change users roles.
    //TODO: Remove block on signup if blockend (check if works).
    //TODO: Prevent admins from changing their own roles.
    //TODO: Add returnurl to access denied and other relevant pages.
    //TODO: Add admin management page with list of blocked users/topics/threads/posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Add SuperAdmin?
    //TODO: Add areas?

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
      return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>();
    }
  }
}