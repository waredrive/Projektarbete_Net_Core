﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Forum {
  public class Program {
    public static void Main(string[] args) {
      CreateWebHostBuilder(args).Build().Run();
    }

   // TODO: CHeck if all redirections work after change.
    //TODO: Check if access denied page works, inc. redirections.
    //TODO: Add not found to thread and posts to check if topic and thread exist. Use modal to display fail.
    //TODO: Maybe add isauthorized as route access attributes instead of checking in action?
    //TODO: Return values from services to act upon in controllers (i.e. if an entity is not added, display modal failed), can use savechangesasync for this.
    //TODO: https://www.smarterasp.net
    //TODO: Do not update if sending no new data. All edits/updates.
    //TODO: Add seed of Default deleted user if possible?
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