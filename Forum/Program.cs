﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

    //TODO: Add custom messages to AccessDenied (Use TempData).
    //TODO: When going to specific post id, make it so you go there with pagination enabled. Now you can only go to the first page.
    //TODO: Relocate the edit/lock/delete buttons that are showing beside the thread/topic names.
    //TODO: There are som redundant ReturnUrl in code. Remove or use!
    //TODO: Add confirmations when adding, deleting, editing, etc. Maybe modal?
    //TODO: Add visual indicator in Topic/Thread/Post that it is blocked. - beside the name
    //TODO: Create Partial views.
    //TODO: Prevent pages add to history, for omitting when back button is pressed. Only "nested" pages - Profile and Account.
    //TODO: Mark aktive page in statistics panel (forumManagement).
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