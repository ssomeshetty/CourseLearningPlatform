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
            ImageUrl = command.ImageUrl,
            Cost = command.Cost,
            Hours = command.Hours,
            CreatedAt = DateTime.UtcNow
        };
        return await courseRepository.AddAsync(course, cancellationToken);
    }
}


public sealed class UpdateCourseHandler(ICourseRepository courseRepository)
{
    public async Task<Course?> HandleAsync(int id, UpdateCourseCommand command,
        CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetByIdAsync(id, cancellationToken);
        if (course is null)
        {
            return null;

        }

        course.Title = command.Title;
        course.Description = command.Description;
        course.Instructor = command.Instructor;
        course.ImageUrl = command.ImageUrl;
        course.Cost = command.Cost;
        course.Hours = command.Hours;

        await courseRepository.UpdateAsync(course, cancellationToken);
        return course;

    }
}

public sealed class DeleteCourseHandler(ICourseRepository courseRepository)
{
    public async Task<Course?> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await courseRepository.GetByIdAsync(id, cancellationToken);

        if (course is null)
        {
            return null;

        }

        await courseRepository.DeleteAsync(course, cancellationToken);
        return course;

    }
}
