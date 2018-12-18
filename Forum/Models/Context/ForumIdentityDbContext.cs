using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Context {
 public class ForumIdentityDbContext : IdentityDbContext<IdentityUser>{
   public ForumIdentityDbContext(DbContextOptions<ForumIdentityDbContext> options) : base(options) {
   }
  }
}
