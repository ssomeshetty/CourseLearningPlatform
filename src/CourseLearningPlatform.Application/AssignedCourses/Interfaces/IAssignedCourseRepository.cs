using CourseLearningPlatform.Domain.AssignedCourses;

namespace CourseLearningPlatform.Application.AssignedCourses.Interfaces;

public interface IAssignedCourseRepository
{
    Task<AssignedCourse> AddAsync(
        AssignedCourse assignedCourse,
        CancellationToken cancellationToken = default);

    Task<AssignedCourse?> GetByIdAsync(
        int userId,
        int courseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignedCourse>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignedCourse>> GetByCourseIdAsync(
        int courseId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        AssignedCourse assignedCourse,
        CancellationToken cancellationToken = default);
}
