using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Application;
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

    public async Task<PagedResult<Course>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Courses.AsNoTracking().OrderBy(course => course.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var courses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>(courses, page, pageSize, totalCount);
    }

    public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        context.Courses.Update(course);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Course course, CancellationToken cancellationToken = default)
    {
        context.Courses.Remove(course);
        await context.SaveChangesAsync(cancellationToken);
    }

}
