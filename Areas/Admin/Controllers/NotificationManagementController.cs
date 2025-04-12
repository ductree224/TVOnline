using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Services;

namespace TVOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<Users> _userManager;

        public NotificationManagementController(
            AppDbContext context,
            INotificationService notificationService,
            UserManager<Users> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: Admin/NotificationManagement
        public async Task<IActionResult> Index(string userId = null, string status = null)
        {
            ViewBag.Users = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            ViewBag.SelectedUserId = userId;
            ViewBag.SelectedStatus = status;

            // Khởi tạo truy vấn cơ bản 
            IQueryable<Notification> notificationsQuery = _context.Notifications
                .Include(n => n.User);
            
            // Lọc theo người dùng cụ thể nếu có
            if (!string.IsNullOrEmpty(userId))
            {
                notificationsQuery = notificationsQuery.Where(n => n.UserId == userId);
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(status))
            {
                bool isRead = status == "read";
                notificationsQuery = notificationsQuery.Where(n => n.IsRead == isRead);
            }

            // Sắp xếp theo thời gian tạo mới nhất trước
            var notifications = await notificationsQuery
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Lấy số lượng yêu cầu nhà tuyển dụng đang chờ xử lý
            ViewBag.PendingEmployerRequests = await _context.EmployerRegistrationRequests
                .Where(r => r.Status == "Pending")
                .CountAsync();

            // Thêm thống kê về thông báo
            ViewBag.TotalNotifications = notifications.Count;
            ViewBag.UnreadNotifications = notifications.Count(n => !n.IsRead);
            ViewBag.ReadNotifications = notifications.Count(n => n.IsRead);

            return View(notifications);
        }

        // GET: Admin/NotificationManagement/EmployerRequests
        public async Task<IActionResult> EmployerRequests()
        {
            var pendingRequests = await _context.EmployerRegistrationRequests
                .Include(r => r.User)
                .Include(r => r.City)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(pendingRequests);
        }

        // GET: Admin/NotificationManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Admin/NotificationManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Users = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            return View();
        }

        // POST: Admin/NotificationManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Title,Message,Link")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                // Sử dụng INotificationService để gửi thông báo
                await _notificationService.SendNotificationAsync(
                    notification.UserId,
                    notification.Title,
                    notification.Message,
                    notification.Link
                );
                
                TempData["SuccessMessage"] = "Thông báo đã được gửi thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            return View(notification);
        }

        // GET: Admin/NotificationManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            ViewBag.Users = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            return View(notification);
        }

        // POST: Admin/NotificationManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Title,Message,Link,IsRead,CreatedAt")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            return View(notification);
        }

        // GET: Admin/NotificationManagement/CreateSystemNotification
        public IActionResult CreateSystemNotification()
        {
            return View();
        }

        // POST: Admin/NotificationManagement/CreateSystemNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSystemNotification([Bind("Title,Message,Link")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var users = await _userManager.Users.ToListAsync();
                    var notifications = new List<Notification>();

                    foreach (var user in users)
                    {
                        notifications.Add(new Notification
                        {
                            UserId = user.Id,
                            Title = notification.Title,
                            Message = notification.Message,
                            Link = notification.Link,
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                    }

                    await _context.Notifications.AddRangeAsync(notifications);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Đã gửi thông báo thành công đến {users.Count} người dùng!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                }
            }
            return View(notification);
        }

        // POST: Admin/NotificationManagement/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/NotificationManagement/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/NotificationManagement/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead(string userId)
        {
            try
            {
                // Nếu có userId, chỉ đánh dấu đã đọc các thông báo của người dùng đó
                if (!string.IsNullOrEmpty(userId))
                {
                    await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                    TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo của người dùng là đã đọc.";
                }
                else
                {
                    // Đánh dấu tất cả thông báo trong hệ thống là đã đọc
                    var notifications = await _context.Notifications
                        .Where(n => !n.IsRead)
                        .ToListAsync();

                    foreach (var notification in notifications)
                    {
                        notification.IsRead = true;
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { userId });
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }

        // GET: Admin/NotificationManagement/ProcessEmployerRequest
        public async Task<IActionResult> ProcessEmployerRequest(string id)
        {
            if (string.IsNullOrEmpty(id))
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

            // Kiểm tra trạng thái yêu cầu
            if (request.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Yêu cầu này đã được xử lý trước đó.";
                return RedirectToAction(nameof(EmployerRequests));
            }

            return View(request);
        }

        // POST: Admin/NotificationManagement/ApproveEmployerRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEmployerRequest(string id, string message)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

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
                return RedirectToAction(nameof(EmployerRequests));
            }

            try
            {
                // Cập nhật trạng thái yêu cầu
                request.Status = "Approved";
                request.ProcessedAt = DateTime.Now;
                request.AdminNotes = message;

                // Thêm vai trò Employer cho người dùng
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, "Employer");
                }

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Gửi thông báo cho người dùng
                await _notificationService.SendNotificationAsync(
                    request.UserId,
                    "Yêu cầu đăng ký nhà tuyển dụng đã được chấp nhận",
                    $"Yêu cầu đăng ký nhà tuyển dụng của bạn đã được chấp nhận. {message}",
                    "/Employer/Dashboard"
                );

                TempData["SuccessMessage"] = "Đã phê duyệt yêu cầu đăng ký nhà tuyển dụng thành công.";
                return RedirectToAction(nameof(EmployerRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xử lý yêu cầu: {ex.Message}";
                return RedirectToAction(nameof(EmployerRequests));
            }
        }

        // POST: Admin/NotificationManagement/RejectEmployerRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEmployerRequest(string id, string message)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

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
                return RedirectToAction(nameof(EmployerRequests));
            }

            try
            {
                // Cập nhật trạng thái yêu cầu
                request.Status = "Rejected";
                request.ProcessedAt = DateTime.Now;
                request.AdminNotes = message;

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Gửi thông báo cho người dùng
                await _notificationService.SendNotificationAsync(
                    request.UserId,
                    "Yêu cầu đăng ký nhà tuyển dụng đã bị từ chối",
                    $"Yêu cầu đăng ký nhà tuyển dụng của bạn đã bị từ chối. {message}",
                    "/Employer/Register"
                );

                TempData["SuccessMessage"] = "Đã từ chối yêu cầu đăng ký nhà tuyển dụng.";
                return RedirectToAction(nameof(EmployerRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xử lý yêu cầu: {ex.Message}";
                return RedirectToAction(nameof(EmployerRequests));
            }
        }
    }
} 