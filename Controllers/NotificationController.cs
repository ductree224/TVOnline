using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Services;
using TVOnline.ViewModels.Notification;

namespace TVOnline.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<Users> _userManager;

        public NotificationController(
            AppDbContext context,
            INotificationService notificationService,
            UserManager<Users> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
                
            return View(notifications);
        }

        // GET: Notification/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            
            return Json(new { count });
        }                                                        

        // GET: /Notification/MarkAsRead/5
        [HttpGet]
        public async Task<IActionResult> MarkAsRead(int id, string returnUrl = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications.FindAsync(id);
            
            if (notification == null || notification.UserId != userId)
            {
                TempData["ErrorMessage"] = "Thông báo không tồn tại hoặc bạn không có quyền đánh dấu thông báo này.";
                return RedirectToAction(nameof(Index));
            }
            
            await _notificationService.MarkNotificationAsReadAsync(id);
            
            if (!string.IsNullOrEmpty(notification.Link) && returnUrl == null)
            {
                return Redirect(notification.Link);
            }
            
            return Redirect(returnUrl ?? "/Notification/Index");
        }

        // GET: /Notification/MarkAllAsRead
        [HttpGet]
        public async Task<IActionResult> MarkAllAsRead(string returnUrl = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            
            TempData["SuccessMessage"] = "Tất cả thông báo đã được đánh dấu đã đọc.";
            
            return Redirect(returnUrl ?? "/Notification/Index");
        }

        // GET: Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
                
            var viewModel = new NotificationsViewModel
            {
                Notifications = notifications,
                UnreadCount = await _notificationService.GetUnreadNotificationCountAsync(userId)
            };
            
            return PartialView("_NotificationsPartial", viewModel);
        }
    }
}
