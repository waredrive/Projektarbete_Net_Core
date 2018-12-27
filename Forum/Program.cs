﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add custom messages to AccessDenied (Use TempData)
    //TODO: Add Unblock/Block in when in thread/topic.
    //TODO: Add back buttons in threads and topics.
    //TODO: Add returnurl to access denied and other relevant pages. (don't forget from details to delete, edit, etc.)
    //TODO: Change Block to lock for consistency.
    //TODO: Add admin management page with list of blocked users/topics/threads/posts.
    //TODO: Add pagination.
    //TODO: Add id to posts so users can go to them from ex. latest posts.
    //TODO: Add Typeahead with ajax call for searching.
    //TODO: Styling
    //TODO: Add seed of Default deleted user
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