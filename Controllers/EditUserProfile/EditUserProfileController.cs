using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVOnline.Data;
using TVOnline.Models;
using TVOnline.ViewModels.UserProfile;

namespace TVOnline.Controllers.EditUserProfile {
    [Authorize]
    public class EditUserProfileController : Controller {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly AppDbContext _context;

        public EditUserProfileController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            AppDbContext context) {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index() {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng có mật khẩu hay không
            var hasPassword = await _userManager.HasPasswordAsync(user);
            ViewBag.HasPassword = hasPassword;

            var model = new EditUserProfileViewModel {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.UserCity,
                Job = user.UserJob,
                Dob = user.Dob
            };

            // Lấy thông tin ApplicationCvDetail nếu có
            var cvDetail = await _context.ApplicationCvDetails
                .FirstOrDefaultAsync(d => d.UserId == user.Id);
            
            ViewBag.CvDetail = cvDetail != null ? new ApplicationCvDetailViewModel(cvDetail) : new ApplicationCvDetailViewModel { UserId = user.Id };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(EditUserProfileViewModel model) {
            if (!ModelState.IsValid) {
                ViewBag.HasPassword = await _userManager.HasPasswordAsync(await _userManager.GetUserAsync(User));
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.UserCity = model.City;
            user.UserJob = model.Job;
            user.Dob = model.Dob;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) {
                return RedirectToAction("Index", new { success = "Cập nhật thông tin thành công" });
            }

            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.HasPassword = await _userManager.HasPasswordAsync(user);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCvDetail(ApplicationCvDetailViewModel model) {
            if (!ModelState.IsValid) {
                var user = await _userManager.GetUserAsync(User);
                var profileModel = new EditUserProfileViewModel {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    City = user.UserCity,
                    Job = user.UserJob,
                    Dob = user.Dob
                };
                ViewBag.HasPassword = await _userManager.HasPasswordAsync(user);
                ViewBag.CvDetail = model;
                ViewBag.CvDetailError = "Vui lòng kiểm tra lại thông tin";
                return View("Index", profileModel);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem đã có thông tin CV chi tiết chưa
            var existingCvDetail = await _context.ApplicationCvDetails
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (existingCvDetail == null) {
                // Tạo mới nếu chưa có
                var newCvDetail = model.ToModel();
                newCvDetail.UserId = userId;
                newCvDetail.CreatedAt = DateTime.Now;
                
                _context.ApplicationCvDetails.Add(newCvDetail);
            } else {
                // Cập nhật nếu đã có
                existingCvDetail = model.ToModel(existingCvDetail);
                existingCvDetail.UpdatedAt = DateTime.Now;
                
                _context.ApplicationCvDetails.Update(existingCvDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { success = "Cập nhật thông tin CV thành công" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfilePasswordChangeViewModel model) {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return RedirectToAction("Login", "Account");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (model.NewPassword != model.ConfirmNewPassword) {
                return RedirectToAction("Index", new { error = "Mật khẩu xác nhận không khớp" });
            }

            if (model.NewPassword.Length < 6) {
                return RedirectToAction("Index", new { error = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            if (!hasPassword) {
                // Nếu chưa có mật khẩu, tạo mật khẩu mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (addPasswordResult.Succeeded) {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", new { success = "Tạo mật khẩu thành công" });
                }

                var errorMessage = addPasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể tạo mật khẩu. Vui lòng thử lại.";
                return RedirectToAction("Index", new { error = errorMessage });
            }

            // Nếu đã có mật khẩu, kiểm tra mật khẩu hiện tại
            if (string.IsNullOrEmpty(model.CurrentPassword)) {
                return RedirectToAction("Index", new { error = "Vui lòng nhập mật khẩu hiện tại" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (changePasswordResult.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", new { success = "Đổi mật khẩu thành công" });
            }

            var changePasswordError = changePasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể đổi mật khẩu. Vui lòng thử lại.";
            return RedirectToAction("Index", new { error = changePasswordError });
        }
    }
}