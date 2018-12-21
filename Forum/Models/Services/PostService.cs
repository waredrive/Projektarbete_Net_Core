using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Forum.Models.Context;
using Forum.Models.Entities;
using Forum.Models.ViewModels.PostViewModel;
using Forum.Models.ViewModels.PostViewModels;
using Microsoft.AspNetCore.Identity;

namespace Forum.Models.Services
{
    public class PostService
    {
      private readonly ForumDbContext _db;
      private readonly SignInManager<IdentityUser> _signInManager;
      private readonly UserManager<IdentityUser> _userManager;

      public PostService(ForumDbContext db, UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager) {
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
      }

    public async Task Add(PostCreateVm postCreateVm, ClaimsPrincipal user) {
      var currentUserId = _userManager.GetUserId(user);
      if (currentUserId == null)
        return;

      var post = new Post {
        Thread = postCreateVm.ThreadId,
        CreatedBy = currentUserId,
        ContentText = postCreateVm.PostText,
        CreatedOn = DateTime.UtcNow
      };

      _db.Post.Add(post);
      await _db.SaveChangesAsync();
    }

      public async Task<PostsIndexVm> GetTopicsIndexVm(int threadId) {
      var postsIndexVm = new PostsIndexVm {
          Posts = new List<PostsIndexPostVm>()
        };

        postsIndexVm.Posts.AddRange(_db.Post.Where(p => p.Thread == threadId).Select(p => new PostsIndexPostVm {
          PostId = p.Id,
          CreatedOn = (DateTime)p.CreatedOn,
          CreatedBy = _userManager.FindByIdAsync(p.CreatedBy).Result.UserName,
          PostText = p.ContentText
        }));

        return postsIndexVm;
    }
    }
}
