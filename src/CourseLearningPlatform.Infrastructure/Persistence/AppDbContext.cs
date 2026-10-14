using CourseLearningPlatform.Domain.Courses;
using Microsoft.EntityFrameworkCore;

namespace CourseLearningPlatform.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    
    {
        
    }
    public DbSet<Course> Courses => Set<Course>();
}