using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.Persistence.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forum.MVC {
  public class Startup {
    private readonly IConfiguration _config;

    public Startup(IConfiguration config) {
      _config = config;
    }

    public void ConfigureServices(IServiceCollection services) {
      services.AddIdentity<IdentityUser, IdentityRole>();
      services.AddDbContext<ForumContext>(o => o.UseSqlServer(_config.GetConnectionString("ForumDb_Dev")));
      services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseAuthentication();
      app.UseStaticFiles();
      app.UseMvc();
    }
  }
}
