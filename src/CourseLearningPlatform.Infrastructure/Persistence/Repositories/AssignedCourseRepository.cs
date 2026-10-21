using CourseLearningPlatform.Application.AssignedCourses.Interfaces;
using CourseLearningPlatform.Domain.AssignedCourses;
using Microsoft.EntityFrameworkCore;

namespace CourseLearningPlatform.Infrastructure.Persistence.Repositories;

public sealed class AssignedCourseRepository(AppDbContext context) : IAssignedCourseRepository
{
    public async Task<AssignedCourse> AddAsync(
        AssignedCourse assignedCourse,
        CancellationToken cancellationToken = default)
    {
        await context.AssignedCourses.AddAsync(assignedCourse, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return assignedCourse;
    }

    public Task<AssignedCourse?> GetByIdAsync(
        int userId,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return context.AssignedCourses
            .Include(assignment => assignment.User)
            .Include(assignment => assignment.Course)
            .FirstOrDefaultAsync(
            assignment => assignment.UserId == userId && assignment.CourseId == courseId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AssignedCourse>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await context.AssignedCourses
            .AsNoTracking()
            .Include(assignment => assignment.Course)
            .Where(assignment => assignment.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignedCourse>> GetByCourseIdAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return await context.AssignedCourses
            .AsNoTracking()
            .Include(assignment => assignment.User)
            .Where(assignment => assignment.CourseId == courseId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        AssignedCourse assignedCourse,
        CancellationToken cancellationToken = default)
    {
        context.AssignedCourses.Update(assignedCourse);
        await context.SaveChangesAsync(cancellationToken);
    }
}
