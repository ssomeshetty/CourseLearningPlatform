using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Application;

namespace CourseLearningPlatform.Application.Courses.Interfaces;

public interface ICourseRepository
{
    Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default);
    Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<Course>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(Course course, CancellationToken cancellationToken = default);
}
