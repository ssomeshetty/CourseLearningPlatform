using CourseLearningPlatform.Domain.Courses;

namespace CourseLearningPlatform.Application.Courses.Interfaces;

public interface ICourseRepository
{
    Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default);
    Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}