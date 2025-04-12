using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Services;
using TVOnline.ViewModels.Admin;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EmployerRequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly INotificationService _notificationService;

        public EmployerRequestsController(
            AppDbContext context,
            UserManager<Users> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Admin/EmployerRequests
        public async Task<IActionResult> Index(string status = "Pending")
        {
            ViewBag.CurrentStatus = status;
            
            var requests = await _context.EmployerRegistrationRequests
                .Include(r => r.User)
                .Include(r => r.City)
                .Where(r => status == "All" || r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
                
            return View(requests);
        }

        // GET: Admin/EmployerRequests/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.EmployerRegistrationRequests
                .Include(r => r.User)
                .Include(r => r.City)
                .FirstOrDefaultAsync(r => r.RequestId == id);
                
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Admin/EmployerRequests/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id, string adminNotes)
        {
            var request = await _context.EmployerRegistrationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RequestId == id);
                
            if (request == null)
            {
                return NotFound();
            }
            
            if (request.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Yêu cầu này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Update request status
            request.Status = "Approved";
            request.AdminNotes = adminNotes;
            request.ProcessedAt = DateTime.Now;
            
            // Create employer record
            var employer = new Employers
            {
                EmployerId = Guid.NewGuid().ToString(), // Thêm ID cho Employer
                UserId = request.UserId,
                Email = request.Email, // Đảm bảo Email được thiết lập
                CompanyName = request.CompanyName,
                Description = request.Description,
                Field = request.Field,
                LogoURL = request.LogoURL,
                Website = request.Website,
                CityId = request.CityId,
                CreatedAt = DateTime.Now
            };
            
            // Add employer role to user
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Employer");
                
                // Liên kết user với employer
                user.EmployerId = employer.EmployerId;
                _context.Users.Update(user);
            }
            
            // Save changes
            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();
            
            // Send notification to user
            await _notificationService.SendNotificationAsync(
                request.UserId,
                "Đăng ký nhà tuyển dụng thành công",
                "Yêu cầu đăng ký trở thành nhà tuyển dụng của bạn đã được chấp thuận. Bạn có thể bắt đầu đăng tin tuyển dụng ngay bây giờ.",
                "/EmployerDashboard/Index"
            );
            
            TempData["SuccessMessage"] = "Đã chấp thuận yêu cầu và tạo tài khoản nhà tuyển dụng thành công.";
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Admin/EmployerRequests/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, string adminNotes)
        {
            var request = await _context.EmployerRegistrationRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);
                
            if (request == null)
            {
                return NotFound();
            }
            
            if (request.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Yêu cầu này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            // Update request status
            request.Status = "Rejected";
            request.AdminNotes = adminNotes;
            request.ProcessedAt = DateTime.Now;
            
            // Save changes
            await _context.SaveChangesAsync();
            
            // Send notification to user
            await _notificationService.SendNotificationAsync(
                request.UserId,
                "Yêu cầu đăng ký nhà tuyển dụng bị từ chối",
                "Yêu cầu đăng ký trở thành nhà tuyển dụng của bạn đã bị từ chối. Vui lòng liên hệ với quản trị viên để biết thêm chi tiết.",
                "/EmployerRegistration/Status"
            );
            
            TempData["SuccessMessage"] = "Đã từ chối yêu cầu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
