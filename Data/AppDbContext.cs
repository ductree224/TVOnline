using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TVOnline.Models;
using static TVOnline.Models.Location;

namespace TVOnline.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {

        public DbSet<Employers> Employers { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Feedbacks> Feedbacks { get; set; }
        public DbSet<UserCV> UserCVs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PremiumUser> PremiumUsers { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<CVTemplate> CVTemplates { get; set; }
        public DbSet<AccountStatus> AccountStatuses { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<PremiumUserCV> PremiumUserCVs { get; set; }
        public DbSet<EmployerRegistrationRequest> EmployerRegistrationRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.UserCVs)          // Một User có nhiều UserCV  
                .WithOne(cv => cv.Users)         // Một UserCV thuộc về một User  
                .HasForeignKey(cv => cv.UserId); // Chỉ định khóa ngoại UserId trong UserCV  

            // Employers relationships
            modelBuilder.Entity<Employers>()
                .HasOne(e => e.City)
                .WithMany(c => c.Employers)
                .HasForeignKey(e => e.CityId);

            modelBuilder.Entity<Employers>()
                .HasMany(e => e.Posts)
                .WithOne(p => p.Employer)
                .HasForeignKey(p => p.EmployerId);

            // Users - Employers one-to-one relationship
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Employer)
                .WithOne(e => e.User)
                .HasForeignKey<Employers>(e => e.UserId);

            // Cities and Zone relationships
            modelBuilder.Entity<Cities>()
                .HasOne(c => c.Zone)
                .WithMany(z => z.Cities)
                .HasForeignKey(c => c.ZoneId);

            // Feedback relationships
            modelBuilder.Entity<Feedbacks>()
                .HasOne(f => f.User)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.UserId);

            // Thiết lập các ràng buộc
            modelBuilder.Entity<Users>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<Cities>()
                .Property(c => c.CityName)
                .IsRequired();

            modelBuilder.Entity<Zone>()
                .Property(z => z.ZoneName)
                .IsRequired();

            // Employer properties
            modelBuilder.Entity<Employers>()
                .Property(e => e.CompanyName)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Email)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Description)
                .IsRequired();

            modelBuilder.Entity<Employers>()
                .Property(e => e.Field)
                .IsRequired();

            // cấu hình cho các khóa chính là string


            modelBuilder.Entity<Cities>()
                .Property(c => c.CityId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Zone>()
                .Property(z => z.ZoneId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Post>()
                .Property(p => p.PostId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Feedbacks>()
                .Property(f => f.FeedbackId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");



            modelBuilder.Entity<UserCV>()
                .Property(cv => cv.CvID)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentId).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Status).HasColumnName("Status");
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PremiumUser>()
                .Property(pu => pu.PremiumUserId)
                .ValueGeneratedOnAdd();


        }
    }
}