using System;
using Microsoft.EntityFrameworkCore;

namespace Forum.Persistence {
  public class ForumDbContext : DbContext{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options) {
      
    }
  }
}
