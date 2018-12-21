﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.ThreadViewModels;
using Microsoft.AspNetCore.Identity;

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
        CreatedBy = currentUserId,
        ContentText = threadCreateVm.ThreadText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Thread.Add(thread);
      await _db.SaveChangesAsync();
    }

    public async Task<ThreadsIndexVm> GetThreadsIndexVm() {
      var threadsIndexVm = new ThreadsIndexVm {
        Threads = new List<ThreadsIndexThreadVm>()
      };

      threadsIndexVm.Threads.AddRange(_db.Thread.Select(t => new ThreadsIndexThreadVm() {
        ThreadId = t.Id,
        CreatedOn = (DateTime)t.CreatedOn,
        CreatedBy = _userManager.FindByIdAsync(t.CreatedBy).Result.UserName,
        ThreadText = t.ContentText
      }));

      return threadsIndexVm;
    }

  }
}
