using System;
using Forum.Models.Context;
using Forum.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Forum {
  public class Startup {
    private readonly IConfiguration _config;

    public Startup(IConfiguration config) {
      _config = config;
    }

    public void ConfigureServices(IServiceCollection services) {
      services.AddDbContext<ForumDbContext>(o => o.UseSqlServer(_config.GetConnectionString("ForumDb_Dev")));
      services.AddDbContext<IdentityDbContext>(o =>
        o.UseSqlServer(_config.GetConnectionString("ForumDb_Dev")));
      services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();
      services.Configure<IdentityOptions>(o => o.User.RequireUniqueEmail = true);
      services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);
      services.AddScoped<AuthorizationService>();
      services.AddScoped<AccountService>();
      services.AddScoped<TopicService>();
      services.AddScoped<ThreadService>();
      services.AddScoped<PostService>();
      services.AddScoped<ProfileService>();
      services.AddScoped<SharedService>();
      services.AddScoped<ForumManagementService>();
      services.AddMvc(o => {
        var policy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();
        o.Filters.Add(new AuthorizeFilter(policy));
      });
      services.AddRouting(options => options.LowercaseUrls = true);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
      if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();

      app.UseHttpsRedirection();
      app.UseAuthentication();
      app.UseStaticFiles();
      app.UseMvc();
    }
  }
}