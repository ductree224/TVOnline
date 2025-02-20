using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVOnline.Models;
using System.Collections.Generic;

namespace TVOnline.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("[action]")]
        [Route("/")]
        public IActionResult Index() {
        
            // Tạo danh sách tạm thời các bài đăng
            var posts = new List<Post>
            {
                new Post { PostId = 1, Description = "Đây là bài đăng 1", EmployerId = 101, Date = DateTime.Now.AddDays(-1) },
                new Post { PostId = 2, Description = "Mô tả bài đăng 2", EmployerId = 102, Date = DateTime.Now.AddDays(-2) },
                new Post { PostId = 3, Description = "Mô tả bài đăng 3", EmployerId = 103, Date = DateTime.Now.AddDays(-3) },
                new Post { PostId = 3, Description = "Mô tả bài đăng 3", EmployerId = 103, Date = DateTime.Now.AddDays(-3) },
                new Post { PostId = 3, Description = "Mô tả bài đăng 3", EmployerId = 103, Date = DateTime.Now.AddDays(-3) },
                new Post { PostId = 3, Description = "Mô tả bài đăng 3", EmployerId = 103, Date = DateTime.Now.AddDays(-3) },
            };

            return View(posts);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
