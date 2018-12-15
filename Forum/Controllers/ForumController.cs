using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Forum.MVC.Controllers
{
  [Route("")]
  [Route("Forum")]
    public class ForumController : Controller
    {
      [Route("")]
      [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}