using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Application.Courses.Interfaces;
using CourseLearningPlatform.Application.Courses.Queries;

namespace CourseLearningPlatform.Application.Courses.Handlers;

public sealed class CourseQueryHandler(ICourseRepository courseRepository)
{
    public Task<Course?> GetByIdAsync(
        GetCourseByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return courseRepository.GetByIdAsync(query.Id, cancellationToken);
    }

    public Task<IReadOnlyList<Course>> GetAllAsync(
        GetCoursesQuery query,
        CancellationToken cancellationToken = default)
    {
        return courseRepository.GetAllAsync(cancellationToken);
    }
}