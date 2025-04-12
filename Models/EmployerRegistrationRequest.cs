using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TVOnline.Models
{
    public class EmployerRegistrationRequest
    {
        [Key]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        [ValidateNever]
        public virtual Users User { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string CompanyName { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public string Field { get; set; }
        
        [NotMapped]
        public IFormFile? Logo { get; set; }
        
        public string? LogoURL { get; set; }
        
        public string? Website { get; set; }
        
        [Required]
        public int CityId { get; set; }
        
        [ForeignKey("CityId")]
        [ValidateNever]
        public Location.Cities? City { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        
        public string? AdminNotes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? ProcessedAt { get; set; }
    }
}
