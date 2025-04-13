using System.ComponentModel.DataAnnotations;
using TVOnline.Models;

namespace TVOnline.ViewModels.UserProfile
{
    public class ApplicationCvDetailViewModel
    {
        public string UserId { get; set; }
        
        [Display(Name = "Học vấn")]
        public string? Education { get; set; }
        
        [Display(Name = "Trường học")]
        public string? School { get; set; }
        
        [Display(Name = "Chuyên ngành")]
        public string? Major { get; set; }
        
        [Display(Name = "Bằng cấp")]
        public string? Degree { get; set; }
        
        [Display(Name = "Năm tốt nghiệp")]
        [Range(1950, 2050, ErrorMessage = "Năm tốt nghiệp phải nằm trong khoảng từ 1950 đến 2050")]
        public int? GraduationYear { get; set; }

        [Display(Name = "Kinh nghiệm làm việc")]
        public string? WorkExperience { get; set; }
        
        [Display(Name = "Công ty gần nhất")]
        public string? LastCompany { get; set; }
        
        [Display(Name = "Vị trí gần nhất")]
        public string? LastPosition { get; set; }
        
        [Display(Name = "Số năm kinh nghiệm")]
        [Range(0, 50, ErrorMessage = "Số năm kinh nghiệm phải nằm trong khoảng từ 0 đến 50")]
        public int? YearsOfExperience { get; set; }
        
        [Display(Name = "Kỹ năng")]
        public string? Skills { get; set; }
        
        [Display(Name = "Chứng chỉ")]
        public string? Certificates { get; set; }
        
        [Display(Name = "Ngôn ngữ")]
        public string? Languages { get; set; }
        
        [Display(Name = "Mức lương mong muốn")]
        [Range(0, 1000000000, ErrorMessage = "Mức lương không hợp lệ")]
        public decimal? ExpectedSalary { get; set; }
        
        [Display(Name = "Vị trí mong muốn")]
        public string? PreferredJobTitle { get; set; }
        
        [Display(Name = "Thông tin bổ sung")]
        public string? AdditionalInfo { get; set; }
        
        // Constructor để chuyển đổi từ model sang viewmodel
        public ApplicationCvDetailViewModel() { }
        
        public ApplicationCvDetailViewModel(ApplicationCvDetail model)
        {
            if (model == null) return;
            
            UserId = model.UserId;
            Education = model.Education;
            School = model.School;
            Major = model.Major;
            Degree = model.Degree;
            GraduationYear = model.GraduationYear;
            WorkExperience = model.WorkExperience;
            LastCompany = model.LastCompany;
            LastPosition = model.LastPosition;
            YearsOfExperience = model.YearsOfExperience;
            Skills = model.Skills;
            Certificates = model.Certificates;
            Languages = model.Languages;
            ExpectedSalary = model.ExpectedSalary;
            PreferredJobTitle = model.PreferredJobTitle;
            AdditionalInfo = model.AdditionalInfo;
        }
        
        // Phương thức để chuyển đổi từ viewmodel sang model
        public ApplicationCvDetail ToModel(ApplicationCvDetail? existingModel = null)
        {
            var model = existingModel ?? new ApplicationCvDetail();
            
            model.UserId = UserId;
            model.Education = Education;
            model.School = School;
            model.Major = Major;
            model.Degree = Degree;
            model.GraduationYear = GraduationYear;
            model.WorkExperience = WorkExperience;
            model.LastCompany = LastCompany;
            model.LastPosition = LastPosition;
            model.YearsOfExperience = YearsOfExperience;
            model.Skills = Skills;
            model.Certificates = Certificates;
            model.Languages = Languages;
            model.ExpectedSalary = ExpectedSalary;
            model.PreferredJobTitle = PreferredJobTitle;
            model.AdditionalInfo = AdditionalInfo;
            model.UpdatedAt = DateTime.Now;
            
            return model;
        }
    }
}
