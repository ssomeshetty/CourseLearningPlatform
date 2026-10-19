using CourseLearningPlatform.Domain.AssignedCourses;
using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace CourseLearningPlatform.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    
    {
        
    }
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AssignedCourse> AssignedCourses => Set<AssignedCourse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserProfile>().Property(user => user.JoiningDate).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<UserProfile>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<UserProfile>().HasIndex(user => user.EmployeeCode).IsUnique();
        modelBuilder.Entity<UserProfile>().Property(user => user.IsAdmin).HasDefaultValue(false);

        modelBuilder.Entity<AssignedCourse>(entity =>
        {
            entity.HasKey(assignedCourse => new { assignedCourse.UserId, assignedCourse.CourseId });
            entity.Property(assignedCourse => assignedCourse.HoursCompleted).HasPrecision(8, 2);
            entity.Property(assignedCourse => assignedCourse.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasOne(assignedCourse => assignedCourse.User)
                .WithMany()
                .HasForeignKey(assignedCourse => assignedCourse.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(assignedCourse => assignedCourse.Course)
                .WithMany()
                .HasForeignKey(assignedCourse => assignedCourse.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}



