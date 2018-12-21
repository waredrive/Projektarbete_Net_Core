using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ThreadViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Forum.Models.Services {
  public class ThreadService {
    private readonly ForumDbContext _db;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ThreadService(ForumDbContext db, UserManager<IdentityUser> userManager,
      SignInManager<IdentityUser> signInManager) {
      _db = db;
      _userManager = userManager;
      _signInManager = signInManager;
    }

    public async Task Add(ThreadCreateVm threadCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var thread = new Thread {
        Topic = threadCreateVm.TopicId,
        CreatedBy = currentUserId,
        ContentText = threadCreateVm.ThreadText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Thread.Add(thread);
      await _db.SaveChangesAsync();
    }

    public async Task<ThreadsIndexVm> GetThreadsIndexVm(int topicId) {
      var threadsIndexVm = new ThreadsIndexVm {
        Topic = await _db.Topic.Where(t => t.Id == topicId).Select(t => t.ContentText).FirstOrDefaultAsync(),
        Threads = new List<ThreadsIndexThreadVm>()
      };

      threadsIndexVm.Threads.AddRange(_db.Thread.Where(t => t.Topic == topicId).Select(t => new ThreadsIndexThreadVm() {
        ThreadId = t.Id,
        CreatedOn = t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadText = t.ContentText,
        PostCount = t.Post.Count
      }));

      return threadsIndexVm;
    }

  }
}
