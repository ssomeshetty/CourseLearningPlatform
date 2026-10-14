using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CourseLearningPlatform.Infrastructure.Persistence.Repositories;

public sealed class CourseRepository(AppDbContext context) : ICourseRepository
{
    public async Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        await context.Courses.AddAsync(course, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return course;
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.Courses.FirstOrDefaultAsync(course => course.Id == id, cancellationToken);

    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Courses.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        context.Courses.Update(course);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await context.Courses.FirstOrDefaultAsync(course => course.Id == id, cancellationToken);
        if (course is null)
        {
            return;
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync(cancellationToken);
    }

}