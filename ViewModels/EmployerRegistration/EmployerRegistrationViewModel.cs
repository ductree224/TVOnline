using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TVOnline.ViewModels.EmployerRegistration
{
    public class EmployerRegistrationViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Tên công ty là bắt buộc")]
        [Display(Name = "Tên công ty")]
        public string CompanyName { get; set; }
        
        [Required(ErrorMessage = "Mô tả công ty là bắt buộc")]
        [Display(Name = "Mô tả công ty")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Lĩnh vực hoạt động là bắt buộc")]
        [Display(Name = "Lĩnh vực hoạt động")]
        public string Field { get; set; }
        
        [Display(Name = "Logo công ty")]
        [ValidateNever]
        public IFormFile? Logo { get; set; }
        
        [Display(Name = "Website")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string? Website { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn thành phố")]
        [Display(Name = "Thành phố")]
        public int CityId { get; set; }
    }
}
