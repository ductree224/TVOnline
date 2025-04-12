using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Service.Post;
using TVOnline.Service.UserCVs;
using TVOnline.Services;
using TVOnline.ViewModels.Account;

namespace TVOnline.Controllers {
    public class AccountController : Controller {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserCvService _userCvService;
        private readonly IPostService _postService;
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        private const string PENDING_REGISTRATION_PREFIX = "PendingRegistration_";

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor,
            IMemoryCache memoryCache,
            IUserCvService userCvService,
            IPostService postService,
            AppDbContext context,
            INotificationService notificationService) {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            _configuration = configuration;
            _contextAccessor = contextAccessor;
            _memoryCache = memoryCache;
            _userCvService = userCvService;
            _postService = postService;
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized();
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new {
                    id = n.Id,
                    title = n.Title,
                    message = n.Message,
                    link = n.Link,
                    isRead = n.IsRead,
                    createdAt = n.CreatedAt
                })
                .ToListAsync();

            return Json(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadNotificationCount() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                return Json(0);
            }

            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized();
            }

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (notification == null) {
                return NotFound();
            }

            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Notifications() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                return RedirectToAction("Login");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsAsRead() {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized();
            }

            await _notificationService.MarkAllNotificationsAsReadAsync(userId);
            return RedirectToAction(nameof(Notifications));
        }
    }
}