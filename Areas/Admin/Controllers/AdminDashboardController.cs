using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TVOnline.Areas.Admin.Models;
using TVOnline.Data;
using TVOnline.Models;
using Microsoft.AspNetCore.Authentication;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminDashboardController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context) {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index() {
            // Lấy tháng và năm hiện tại
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            // Trang dashboard cho admin
            var dashboardViewModel = new AdminDashboardViewModel {
                TotalUsers = _userManager.Users.Count(),
                TotalEmployers = _context.Employers.Count(),
                TotalJobSeekers = _userManager.GetUsersInRoleAsync("JobSeeker").Result.Count,
                TotalPosts = _context.Posts.Count(),
                
                // Thêm số lượng yêu cầu đăng ký nhà tuyển dụng đang chờ xử lý
                PendingEmployerRequests = _context.EmployerRegistrationRequests
                    .Count(r => r.Status == "Pending"),
                
                // Thêm thông tin về người dùng Premium
                TotalPremiumUsers = _context.AccountStatuses
                    .Count(a => a.IsPremium),
                
                // Thêm thông tin về người dùng mới trong tháng này
                // Vì Users không có CreatedAt, chúng ta sẽ đếm số lượng người dùng đăng ký trong tháng này
                // từ bảng AspNetUserTokens hoặc bỏ qua thông tin này
                NewUsersThisMonth = 0,
                
                // Thêm thông tin về bài đăng mới trong tháng này
                NewPostsThisMonth = _context.Posts
                    .Count(p => p.CreatedAt.Month == currentMonth && p.CreatedAt.Year == currentYear)
            };

            return View(dashboardViewModel);
        }

        public async Task<IActionResult> Logout() {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
}
