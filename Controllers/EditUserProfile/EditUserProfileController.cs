using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using TVOnline.Models;
using TVOnline.ViewModels.UserProfile;

namespace TVOnline.Controllers.EditUserProfile
{
    [Authorize]
    public class EditUserProfileController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditUserProfileController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra xem người dùng có mật khẩu hay không
            var hasPassword = await _userManager.HasPasswordAsync(user);
            ViewBag.HasPassword = hasPassword;

            var model = new EditUserProfileViewModel
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.UserCity,
                Job = user.UserJob,
                Dob = user.Dob,
                CurrentCvUrl = user.CvFileUrl,
                Biography = user.Biography,
                Skills = user.Skills,
                WorkExperience = user.WorkExperience,
                Education = user.Education
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(EditUserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HasPassword = await _userManager.HasPasswordAsync(await _userManager.GetUserAsync(User));
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.Name;
            user.PhoneNumber = model.PhoneNumber;
            user.UserCity = model.City;
            user.UserJob = model.Job;
            user.Dob = model.Dob;
            user.Biography = model.Biography;
            user.Skills = model.Skills;
            user.WorkExperience = model.WorkExperience;
            user.Education = model.Education;

            // Handle CV file upload
            if (model.CvFile != null && model.CvFile.Length > 0)
            {
                // Delete old CV file if it exists
                if (!string.IsNullOrEmpty(user.CvFileUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cv", user.CvFileUrl);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Create directory if it doesn't exist
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cv");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(model.CvFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CvFile.CopyToAsync(fileStream);
                }

                // Update user CV URL and last update time
                user.CvFileUrl = uniqueFileName;
                user.LastCvUpdate = DateTime.Now;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { success = "Cập nhật thông tin thành công" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.HasPassword = await _userManager.HasPasswordAsync(user);
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfilePasswordChangeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return RedirectToAction("Index", new { error = "Mật khẩu xác nhận không khớp" });
            }

            if (model.NewPassword.Length < 6)
            {
                return RedirectToAction("Index", new { error = "Mật khẩu phải có ít nhất 6 ký tự" });
            }

            if (!hasPassword)
            {
                // Nếu chưa có mật khẩu, tạo mật khẩu mới
                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (addPasswordResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", new { success = "Tạo mật khẩu thành công" });
                }

                var errorMessage = addPasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể tạo mật khẩu. Vui lòng thử lại.";
                return RedirectToAction("Index", new { error = errorMessage });
            }

            // Nếu đã có mật khẩu, kiểm tra mật khẩu hiện tại
            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                return RedirectToAction("Index", new { error = "Vui lòng nhập mật khẩu hiện tại" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", new { success = "Đổi mật khẩu thành công" });
            }

            var changePasswordError = changePasswordResult.Errors.FirstOrDefault()?.Description ?? "Không thể đổi mật khẩu. Vui lòng thử lại.";
            return RedirectToAction("Index", new { error = changePasswordError });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCV()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.CvFileUrl))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cv", user.CvFileUrl);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(user.CvFileUrl);
            
            // Determine content type based on file extension
            var contentType = "application/octet-stream";
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            switch (extension)
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
            }

            return File(fileBytes, contentType, fileName);
        }
    }
}
