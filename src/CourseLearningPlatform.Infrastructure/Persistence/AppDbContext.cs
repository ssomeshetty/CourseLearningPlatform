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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserProfile>().Property(user => user.JoiningDate).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<UserProfile>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<UserProfile>().HasIndex(user => user.EmployeeCode).IsUnique();
        modelBuilder.Entity<UserProfile>().Property(user => user.IsAdmin).HasDefaultValue(false);

    }
}

