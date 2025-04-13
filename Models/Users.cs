using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TVOnline.Models.Vnpay;

namespace TVOnline.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }
        public string? UserCity { get; set; }
        public string? UserJob { get; set; }
        public DateTime? Dob { get; set; }
        
        // New profile fields
        public string? CvFileUrl { get; set; }
        public string? Biography { get; set; }
        public string? Skills { get; set; }
        public string? WorkExperience { get; set; }
        public string? Education { get; set; }
        public DateTime? LastCvUpdate { get; set; }

        [InverseProperty("User")]
        public virtual PremiumUser? PremiumUser { get; set; }

        [InverseProperty("User")]
        public virtual ApplicationCvDetail? ApplicationCvDetail { get; set; }

        public virtual Employers? Employer { get; set; }
        public string? EmployerId { get; set; }
        public virtual ICollection<Feedbacks>? Feedbacks { get; set; }
        public virtual ICollection<PaymentInformationModel>? Payments { get; set; }
        public virtual ICollection<UserCV>? UserCVs { get; set; }
    }
}