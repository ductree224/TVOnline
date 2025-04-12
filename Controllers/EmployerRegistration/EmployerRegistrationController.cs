using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.Service.Location;
using TVOnline.ViewModels.EmployerRegistration;

namespace TVOnline.Controllers.EmployerRegistration
{
    [Authorize]
    public class EmployerRegistrationController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILocationService _locationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployerRegistrationController(
            UserManager<Users> userManager,
            AppDbContext context,
            ILocationService locationService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _locationService = locationService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: EmployerRegistration
        public async Task<IActionResult> Index()
        {
            return await Register();
        }
        
        // GET: EmployerRegistration/Register
        public async Task<IActionResult> Register()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                // Check if user is already an employer
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
                if (isEmployer)
                {
                    TempData["InfoMessage"] = "Bạn đã là nhà tuyển dụng.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                
                // Check if user already has a pending request
                var pendingRequest = await _context.EmployerRegistrationRequests
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == "Pending");
                    
                if (pendingRequest != null)
                {
                    return View("RequestPending", pendingRequest);
                }
                
                var viewModel = new EmployerRegistrationViewModel
                {
                    Email = user.Email
                };
                
                ViewData["Title"] = "Đăng ký trở thành nhà tuyển dụng";
                return View("Register", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Register action: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang đăng ký. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: EmployerRegistration/GetCities
        [HttpGet]
        public async Task<IActionResult> GetCities()
        {
            try
            {
                var cities = await _locationService.GetAllCities();
                
                // Chuyển đổi dữ liệu để phù hợp với cấu trúc mà client đang mong đợi
                var formattedCities = cities.Select(c => new 
                {
                    cityId = c.Id,
                    cityName = c.Name
                }).ToList();
                
                return Json(formattedCities);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetCities action: {ex.Message}");
                return Json(new List<object>());
            }
        }

        // POST: EmployerRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(EmployerRegistrationViewModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                // Check if user is already an employer
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                var isEmployer = await _userManager.IsInRoleAsync(user, "Employer");
                if (isEmployer)
                {
                    TempData["InfoMessage"] = "Bạn đã là nhà tuyển dụng.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                
                // Check if user already has a pending request
                var pendingRequest = await _context.EmployerRegistrationRequests
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == "Pending");
                    
                if (pendingRequest != null)
                {
                    return View("RequestPending", pendingRequest);
                }
                
                if (ModelState.IsValid)
                {
                    var logoUrl = string.Empty;
                    
                    // Handle logo upload
                    if (model.Logo != null && model.Logo.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "employers");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Logo.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Logo.CopyToAsync(fileStream);
                        }
                        
                        logoUrl = "/uploads/employers/" + uniqueFileName;
                    }
                    
                    // Create registration request
                    var request = new EmployerRegistrationRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Email = model.Email,
                        CompanyName = model.CompanyName,
                        Description = model.Description,
                        Field = model.Field,
                        LogoURL = logoUrl,
                        Website = model.Website,
                        CityId = model.CityId,
                        Status = "Pending",
                        CreatedAt = DateTime.Now
                    };
                    
                    _context.EmployerRegistrationRequests.Add(request);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Yêu cầu đăng ký trở thành nhà tuyển dụng đã được gửi thành công. Vui lòng chờ quản trị viên xét duyệt.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                
                // If we got this far, something failed, redisplay form
                ViewData["Title"] = "Đăng ký trở thành nhà tuyển dụng";
                return View("Register", model);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Submit action: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi yêu cầu. Vui lòng thử lại sau.";
                
                ViewData["Title"] = "Đăng ký trở thành nhà tuyển dụng";
                return View("Register", model);
            }
        }
        
        // GET: EmployerRegistration/Status
        public async Task<IActionResult> Status()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                var requests = await _context.EmployerRegistrationRequests
                    .Include(r => r.City)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
                
                ViewData["Title"] = "Trạng thái đăng ký nhà tuyển dụng";
                return View(requests);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in Status action: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang trạng thái. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }
    }
}
