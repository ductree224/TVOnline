using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class ApplicationCvDetail
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }
        
        public virtual Users User { get; set; }

        // Thông tin học vấn
        [MaxLength(500)]
        public string? Education { get; set; }
        
        // Trường học
        [MaxLength(200)]
        public string? School { get; set; }
        
        // Chuyên ngành
        [MaxLength(200)]
        public string? Major { get; set; }
        
        // Bằng cấp
        [MaxLength(200)]
        public string? Degree { get; set; }
        
        // Năm tốt nghiệp
        public int? GraduationYear { get; set; }

        // Kinh nghiệm làm việc
        [MaxLength(1000)]
        public string? WorkExperience { get; set; }
        
        // Công ty gần nhất
        [MaxLength(200)]
        public string? LastCompany { get; set; }
        
        // Vị trí gần nhất
        [MaxLength(200)]
        public string? LastPosition { get; set; }
        
        // Số năm kinh nghiệm
        public int? YearsOfExperience { get; set; }
        
        // Kỹ năng
        [MaxLength(500)]
        public string? Skills { get; set; }
        
        // Chứng chỉ
        [MaxLength(500)]
        public string? Certificates { get; set; }
        
        // Ngôn ngữ
        [MaxLength(200)]
        public string? Languages { get; set; }
        
        // Mức lương mong muốn
        public decimal? ExpectedSalary { get; set; }
        
        // Vị trí mong muốn
        [MaxLength(200)]
        public string? PreferredJobTitle { get; set; }
        
        // Thông tin bổ sung
        [MaxLength(1000)]
        public string? AdditionalInfo { get; set; }
        
        // Ngày tạo
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Ngày cập nhật
        public DateTime? UpdatedAt { get; set; }
    }
}
