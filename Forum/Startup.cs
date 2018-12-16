using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forum.MVC.Models.Services;
using Forum.Persistence.Entities.ForumData;
using Forum.Persistence.Entities.ForumIdentityData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
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
      services.AddDbContext<ForumDbContext>(o => o.UseSqlServer(_config.GetConnectionString("ForumDb_Dev")));
      services.AddDbContext<ForumIdentityDbContext>(o => o.UseSqlServer(_config.GetConnectionString("ForumDb_Identity_Dev")));
      services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ForumIdentityDbContext>().AddDefaultTokenProviders();

      services.AddScoped<AccountService>();

      services.AddMvc(o =>
      {
        var policy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();
        o.Filters.Add(new AuthorizeFilter(policy));
      });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {

      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();
      app.UseAuthentication();
      app.UseStaticFiles();
      app.UseMvc();
    }
  }
}
