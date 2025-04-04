using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.UserProfile
{
    public class EditUserProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Họ và tên")]
        public string? Name { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? Dob { get; set; }

        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Chuyên ngành")]
        public string? Job { get; set; }

        [Display(Name = "Tải lên CV")]
        public IFormFile? CvFile { get; set; }

        [Display(Name = "CV hiện tại")]
        public string? CurrentCvUrl { get; set; }
        
        [Display(Name = "Ngày cập nhật CV")]
        public DateTime? LastCvUpdate { get; set; }

        [Display(Name = "Giới thiệu bản thân")]
        [DataType(DataType.MultilineText)]
        public string? Biography { get; set; }

        [Display(Name = "Kỹ năng")]
        public string? Skills { get; set; }

        [Display(Name = "Kinh nghiệm làm việc")]
        [DataType(DataType.MultilineText)]
        public string? WorkExperience { get; set; }

        [Display(Name = "Học vấn")]
        [DataType(DataType.MultilineText)]
        public string? Education { get; set; }
    }
}
