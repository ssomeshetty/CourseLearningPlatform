using CourseLearningPlatform.Domain.Courses;
using CourseLearningPlatform.Application.Courses.Commands;
using CourseLearningPlatform.Application.Courses.Interfaces;

namespace CourseLearningPlatform.Application.Courses.Handlers;

public sealed class CreateCourseHandler(ICourseRepository courseRepository)
{
    public async Task<Course> HandleAsync(CreateCourseCommand command, CancellationToken cancellationToken = default)
    {
        var course = new Course
        {
            Title = command.Title,
            Description = command.Description,
            Instructor = command.Instructor,
            Cost = command.Cost,
            Hours = command.Hours,
            CreatedAt = DateTime.UtcNow
        };
        return await courseRepository.AddAsync(course, cancellationToken);
    }
}